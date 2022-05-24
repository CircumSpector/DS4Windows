using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4Windows.Shared.Common.Types;
using Ds4Windows.Shared.Devices.Interfaces.Output;
using Ds4Windows.Shared.Devices.Interfaces.Services;
using DS4Windows.Shared.Devices.Output;
using Nefarius.ViGEm.Client;

namespace DS4Windows.Shared.Devices.Services
{
    public class OutputSlotManager : IOutputSlotManager
    {
        public static int CURRENT_DS4_CONTROLLER_LIMIT = 8;

        

        public const int DELAY_TIME = 500; // measured in ms

        private readonly Dictionary<int, IOutDevice> deviceDict = new();
        private readonly int lastSlotIndex;
        private readonly IOutDevice[] outputDevices = new IOutDevice[CURRENT_DS4_CONTROLLER_LIMIT];
        private readonly ReaderWriterLockSlim queueLocker;
        private readonly Dictionary<IOutDevice, int> revDeviceDict = new();

        private int queuedTasks;

        public OutputSlotManager(ViGEmClient client)
        {
            Client = client;

            OutputSlots = new IOutSlotDevice[CURRENT_DS4_CONTROLLER_LIMIT];
            for (var i = 0; i < CURRENT_DS4_CONTROLLER_LIMIT; i++) OutputSlots[i] = new OutSlotDevice(i);

            lastSlotIndex = OutputSlots.Count > 0 ? OutputSlots.Count - 1 : 0;

            queueLocker = new ReaderWriterLockSlim();
        }

        public int NumAttachedDevices
        {
            get { return OutputSlots.Count(tmp => tmp.CurrentAttachedStatus == IOutSlotDevice.AttachedStatus.Attached); }
        }

        public ViGEmClient Client { get; }

        public bool RunningQueue => queuedTasks > 0;
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
            while (RunningQueue) Thread.SpinWait(500);

            deviceDict.Clear();
            revDeviceDict.Clear();
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
            queueLocker.EnterWriteLock();
            queuedTasks++;
            //Action tempAction = new Action(() =>
            {
                var slot = FindEmptySlot();
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

                    outputDevices[slot] = outputDevice;
                    deviceDict.Add(slot, outputDevice);
                    revDeviceDict.Add(outputDevice, slot);
                    OutputSlots[slot].AttachedDevice(outputDevice, contType);
                    if (inIdx != -1)
                    {
                        outdevs[inIdx] = outputDevice;
                        OutputSlots[slot].CurrentInputBound = IOutSlotDevice.InputBound.Bound;
                    }

                    SlotAssigned?.Invoke(this, slot, OutputSlots[slot]);
                }
            }

            queuedTasks--;
            queueLocker.ExitWriteLock();
        }

        public void DeferredRemoval(IOutDevice outputDevice, int inIdx,
            IOutDevice[] outdevs, bool immediate = false)
        {
            _ = immediate;

            queueLocker.EnterWriteLock();
            queuedTasks++;

            {
                if (revDeviceDict.TryGetValue(outputDevice, out var slot))
                {
                    //int slot = revDeviceDict[outputDevice];
                    outputDevices[slot] = null;
                    deviceDict.Remove(slot);
                    revDeviceDict.Remove(outputDevice);
                    outputDevice.Disconnect();
                    if (inIdx != -1) outdevs[inIdx] = null;

                    OutputSlots[slot].DetachDevice();
                    SlotUnassigned?.Invoke(this, slot, OutputSlots[slot]);

                    //if (!immediate)
                    //{
                    //    Task.Delay(DELAY_TIME).Wait();
                    //}
                }
            }
            ;

            queuedTasks--;
            queueLocker.ExitWriteLock();
        }

        public IOutSlotDevice FindOpenSlot()
        {
            IOutSlotDevice temp = null;
            for (var i = 0; i < OutputSlots.Count; i++)
            {
                var tmp = OutputSlots[i];
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
                revDeviceDict.TryGetValue(outputDevice, out var slotNum))
                temp = OutputSlots[slotNum];

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

            queueLocker.EnterWriteLock();
            queuedTasks++;
            {
                var slotIdx = 0;
                foreach (var device in OutputSlots)
                {
                    if (device.OutputDevice != null)
                    {
                        outputDevices[slotIdx] = null;
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

            queuedTasks--;
            queueLocker.ExitWriteLock();
        }

        private int FindEmptySlot()
        {
            var result = -1;
            for (var i = 0; i < outputDevices.Length && result == -1; i++)
            {
                var tempdev = outputDevices[i];
                if (tempdev == null) result = i;
            }

            return result;
        }

        public bool SlotAvailable(int slotNum)
        {
            bool result;
            if (slotNum < 0 && slotNum > lastSlotIndex) throw new ArgumentOutOfRangeException("Invalid slot number");

            //slotNum -= 1;
            result = OutputSlots[slotNum].CurrentAttachedStatus == IOutSlotDevice.AttachedStatus.UnAttached;
            return result;
        }

        public IOutSlotDevice GetOutSlotDevice(int slotNum)
        {
            IOutSlotDevice temp;
            if (slotNum < 0 && slotNum > lastSlotIndex) throw new ArgumentOutOfRangeException("Invalid slot number");

            //slotNum -= 1;
            temp = OutputSlots[slotNum];
            return temp;
        }
    }
}
