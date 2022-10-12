using System.ComponentModel;

using Nefarius.ViGEm.Client;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Output;

namespace Vapour.Shared.Devices.Services;

[Obsolete]
public sealed class OutputSlotManager : IOutputSlotManager
{
    public const int DELAY_TIME = 500; // measured in ms
    public static int CURRENT_DS4_CONTROLLER_LIMIT = 8;

    private readonly Dictionary<int, IOutDevice> _deviceDict = new();
    private readonly int _lastSlotIndex;
    private readonly IOutDevice[] _outputDevices = new IOutDevice[CURRENT_DS4_CONTROLLER_LIMIT];
    private readonly ReaderWriterLockSlim _queueLocker;
    private readonly Dictionary<IOutDevice, int> _revDeviceDict = new();

    private int _queuedTasks;

    public OutputSlotManager(ViGEmClient client)
    {
        Client = client;

        OutputSlots = new IOutSlotDevice[CURRENT_DS4_CONTROLLER_LIMIT];
        for (int i = 0; i < CURRENT_DS4_CONTROLLER_LIMIT; i++)
        {
            OutputSlots[i] = new OutSlotDevice(i);
        }

        _lastSlotIndex = OutputSlots.Count > 0 ? OutputSlots.Count - 1 : 0;

        _queueLocker = new ReaderWriterLockSlim();
    }

    public int NumAttachedDevices =>
        OutputSlots.Count(tmp => tmp.CurrentAttachedStatus == IOutSlotDevice.AttachedStatus.Attached);

    public ViGEmClient Client { get; }

    public bool RunningQueue => _queuedTasks > 0;
    public IList<IOutSlotDevice> OutputSlots { get; }

    public event IOutputSlotManager.SlotAssignedDelegate SlotAssigned;
    public event IOutputSlotManager.SlotUnassignedDelegate SlotUnassigned;

    public event EventHandler ViGEmFailure;

    public void ShutDown()
    {
    }

    public void Stop(bool immediate = false)
    {
        UnplugRemainingControllers(immediate);
        while (RunningQueue)
        {
            Thread.SpinWait(500);
        }

        _deviceDict.Clear();
        _revDeviceDict.Clear();
    }

    public IOutDevice AllocateController(OutputDeviceType contType)
    {
        OutDevice outputDevice = null;
        switch (contType)
        {
            case OutputDeviceType.Xbox360Controller:
                outputDevice = new Xbox360OutDevice(Client);
                break;
            case OutputDeviceType.DualShock4Controller:
            //outputDevice = DS4OutDeviceFactory.CreateDS4Device(Client, Global.ViGEmBusVersionInfo);
            //break;
            case OutputDeviceType.None:
            default:
                break;
        }

        return outputDevice;
    }

    public void DeferredPlugin(IOutDevice outputDevice, int inIdx, IOutDevice[] outdevs,
        OutputDeviceType contType)
    {
        _queueLocker.EnterWriteLock();
        _queuedTasks++;
        //Action tempAction = new Action(() =>
        {
            int slot = FindEmptySlot();
            if (slot != -1)
            {
                try
                {
                    outputDevice.Connect();
                }
                catch (Win32Exception)
                {
                    // Leave task immediately if connect call failed
                    ViGEmFailure?.Invoke(this, EventArgs.Empty);
                    return;
                }

                _outputDevices[slot] = outputDevice;
                _deviceDict.Add(slot, outputDevice);
                _revDeviceDict.Add(outputDevice, slot);
                OutputSlots[slot].AttachedDevice(outputDevice, contType);
                if (inIdx != -1)
                {
                    outdevs[inIdx] = outputDevice;
                    OutputSlots[slot].CurrentInputBound = IOutSlotDevice.InputBound.Bound;
                }

                SlotAssigned?.Invoke(this, slot, OutputSlots[slot]);
            }
        }

        _queuedTasks--;
        _queueLocker.ExitWriteLock();
    }

    public void DeferredRemoval(IOutDevice outputDevice, int inIdx,
        IOutDevice[] outdevs, bool immediate = false)
    {
        _ = immediate;

        _queueLocker.EnterWriteLock();
        _queuedTasks++;

        {
            if (_revDeviceDict.TryGetValue(outputDevice, out int slot))
            {
                //int slot = revDeviceDict[outputDevice];
                _outputDevices[slot] = null;
                _deviceDict.Remove(slot);
                _revDeviceDict.Remove(outputDevice);
                outputDevice.Disconnect();
                if (inIdx != -1)
                {
                    outdevs[inIdx] = null;
                }

                OutputSlots[slot].DetachDevice();
                SlotUnassigned?.Invoke(this, slot, OutputSlots[slot]);

                //if (!immediate)
                //{
                //    Task.Delay(DELAY_TIME).Wait();
                //}
            }
        }
        ;

        _queuedTasks--;
        _queueLocker.ExitWriteLock();
    }

    public IOutSlotDevice FindOpenSlot()
    {
        IOutSlotDevice temp = null;
        for (int i = 0; i < OutputSlots.Count; i++)
        {
            IOutSlotDevice tmp = OutputSlots[i];
            if (tmp.CurrentInputBound == IOutSlotDevice.InputBound.Unbound &&
                tmp.CurrentAttachedStatus == IOutSlotDevice.AttachedStatus.UnAttached)
            {
                temp = tmp;
                break;
            }
        }

        return temp;
    }

    public IOutSlotDevice GetOutSlotDevice(IOutDevice outputDevice)
    {
        IOutSlotDevice temp = null;
        if (outputDevice != null &&
            _revDeviceDict.TryGetValue(outputDevice, out int slotNum))
        {
            temp = OutputSlots[slotNum];
        }

        return temp;
    }

    public IOutSlotDevice FindExistUnboundSlotType(OutputDeviceType contType)
    {
        return OutputSlots.FirstOrDefault(tmp =>
            tmp.CurrentInputBound == IOutSlotDevice.InputBound.Unbound &&
            tmp.CurrentAttachedStatus == IOutSlotDevice.AttachedStatus.Attached && tmp.OutputDevice != null &&
            tmp.OutputDevice.GetDeviceType() == contType.ToString());
    }

    public void UnplugRemainingControllers(bool immediate = false)
    {
        _ = immediate;

        _queueLocker.EnterWriteLock();
        _queuedTasks++;
        {
            int slotIdx = 0;
            foreach (IOutSlotDevice device in OutputSlots)
            {
                if (device.OutputDevice != null)
                {
                    _outputDevices[slotIdx] = null;
                    device.OutputDevice.Disconnect();

                    device.DetachDevice();
                    SlotUnassigned?.Invoke(this, slotIdx, OutputSlots[slotIdx]);
                    //if (!immediate)
                    //{
                    //    Task.Delay(DELAY_TIME).Wait();
                    //}
                }

                slotIdx++;
            }
        }

        _queuedTasks--;
        _queueLocker.ExitWriteLock();
    }

    private int FindEmptySlot()
    {
        int result = -1;
        for (int i = 0; i < _outputDevices.Length && result == -1; i++)
        {
            IOutDevice tempdev = _outputDevices[i];
            if (tempdev == null)
            {
                result = i;
            }
        }

        return result;
    }

    public bool SlotAvailable(int slotNum)
    {
        bool result;
        if (slotNum < 0 && slotNum > _lastSlotIndex)
        {
            throw new ArgumentOutOfRangeException("Invalid slot number");
        }

        //slotNum -= 1;
        result = OutputSlots[slotNum].CurrentAttachedStatus == IOutSlotDevice.AttachedStatus.UnAttached;
        return result;
    }

    public IOutSlotDevice GetOutSlotDevice(int slotNum)
    {
        IOutSlotDevice temp;
        if (slotNum < 0 && slotNum > _lastSlotIndex)
        {
            throw new ArgumentOutOfRangeException("Invalid slot number");
        }

        //slotNum -= 1;
        temp = OutputSlots[slotNum];
        return temp;
    }
}