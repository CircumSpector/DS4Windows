using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Control;
using Nefarius.ViGEm.Client;

namespace DS4Windows
{
    public interface IOutputSlotManager
    {
        event OutputSlotManager.SlotAssignedDelegate SlotAssigned;

        event OutputSlotManager.SlotUnassignedDelegate SlotUnassigned;

        ViGEmClient Client { get; }

        event EventHandler ViGEmFailure;

        IList<OutSlotDevice> OutputSlots { get; }

        bool RunningQueue { get; }

        void Stop(bool immediate = false);

        void ShutDown();

        OutputDevice AllocateController(OutContType contType);

        void DeferredPlugin(OutputDevice outputDevice, int inIdx, OutputDevice[] outdevs, OutContType contType);

        void DeferredRemoval(OutputDevice outputDevice, int inIdx,
            OutputDevice[] outdevs, bool immediate = false);

        OutSlotDevice FindOpenSlot();

        OutSlotDevice GetOutSlotDevice(OutputDevice outputDevice);

        OutSlotDevice FindExistUnboundSlotType(OutContType contType);

        void UnplugRemainingControllers(bool immediate = false);
    }

    public class OutputSlotManager : IOutputSlotManager
    {
        public delegate void SlotAssignedDelegate(OutputSlotManager sender,
            int slotNum, OutSlotDevice outSlotDev);

        public delegate void SlotUnassignedDelegate(OutputSlotManager sender,
            int slotNum, OutSlotDevice outSlotDev);

        public const int DELAY_TIME = 500; // measured in ms

        private readonly Dictionary<int, OutputDevice> deviceDict = new();
        private readonly int lastSlotIndex;
        private readonly OutputDevice[] outputDevices = new OutputDevice[ControlService.CURRENT_DS4_CONTROLLER_LIMIT];

        private int queuedTasks;
        private readonly ReaderWriterLockSlim queueLocker;
        private readonly Dictionary<OutputDevice, int> revDeviceDict = new();

        public ViGEmClient Client { get; }

        public OutputSlotManager(ViGEmClient client)
        {
            this.Client = client;

            OutputSlots = new OutSlotDevice[ControlService.CURRENT_DS4_CONTROLLER_LIMIT];
            for (var i = 0; i < ControlService.CURRENT_DS4_CONTROLLER_LIMIT; i++) OutputSlots[i] = new OutSlotDevice(i);

            lastSlotIndex = OutputSlots.Count > 0 ? OutputSlots.Count - 1 : 0;

            queueLocker = new ReaderWriterLockSlim();
        }

        public int NumAttachedDevices
        {
            get
            {
                return OutputSlots.Count(tmp => tmp.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached);
            }
        }

        public bool RunningQueue => queuedTasks > 0;
        public IList<OutSlotDevice> OutputSlots { get; }

        public event SlotAssignedDelegate SlotAssigned;
        public event SlotUnassignedDelegate SlotUnassigned;

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

        public OutputDevice AllocateController(OutContType contType)
        {
            OutputDevice outputDevice = null;
            switch (contType)
            {
                case OutContType.X360:
                    outputDevice = new Xbox360OutDevice(Client);
                    break;
                case OutContType.DS4:
                    outputDevice = DS4OutDeviceFactory.CreateDS4Device(Client, Global.ViGEmBusVersionInfo);
                    break;
                case OutContType.None:
                default:
                    break;
            }

            return outputDevice;
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

        public void DeferredPlugin(OutputDevice outputDevice, int inIdx, OutputDevice[] outdevs, OutContType contType)
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
                        OutputSlots[slot].CurrentInputBound = OutSlotDevice.InputBound.Bound;
                    }

                    SlotAssigned?.Invoke(this, slot, OutputSlots[slot]);
                }
            }

            queuedTasks--;
            queueLocker.ExitWriteLock();
        }

        public void DeferredRemoval(OutputDevice outputDevice, int inIdx,
            OutputDevice[] outdevs, bool immediate = false)
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

        public OutSlotDevice FindOpenSlot()
        {
            OutSlotDevice temp = null;
            for (var i = 0; i < OutputSlots.Count; i++)
            {
                var tmp = OutputSlots[i];
                if (tmp.CurrentInputBound == OutSlotDevice.InputBound.Unbound &&
                    tmp.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached)
                {
                    temp = tmp;
                    break;
                }
            }

            return temp;
        }

        public bool SlotAvailable(int slotNum)
        {
            bool result;
            if (slotNum < 0 && slotNum > lastSlotIndex) throw new ArgumentOutOfRangeException("Invalid slot number");

            //slotNum -= 1;
            result = OutputSlots[slotNum].CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached;
            return result;
        }

        public OutSlotDevice GetOutSlotDevice(int slotNum)
        {
            OutSlotDevice temp;
            if (slotNum < 0 && slotNum > lastSlotIndex) throw new ArgumentOutOfRangeException("Invalid slot number");

            //slotNum -= 1;
            temp = OutputSlots[slotNum];
            return temp;
        }

        public OutSlotDevice GetOutSlotDevice(OutputDevice outputDevice)
        {
            OutSlotDevice temp = null;
            if (outputDevice != null &&
                revDeviceDict.TryGetValue(outputDevice, out var slotNum))
                temp = OutputSlots[slotNum];

            return temp;
        }

        public OutSlotDevice FindExistUnboundSlotType(OutContType contType)
        {
            return OutputSlots.FirstOrDefault(tmp =>
                tmp.CurrentInputBound == OutSlotDevice.InputBound.Unbound &&
                tmp.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached && tmp.OutputDevice != null &&
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
    }
}