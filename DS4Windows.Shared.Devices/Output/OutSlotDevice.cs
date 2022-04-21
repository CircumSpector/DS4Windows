using System;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Shared.Devices.Output
{
    public class OutSlotDevice : IOutSlotDevice
    {
        private IOutSlotDevice.InputBound inputBound;
        private OutputDeviceType permanentType;
        private IOutSlotDevice.ReserveStatus reserveStatus;

        public OutSlotDevice(int idx)
        {
            Index = idx;
            CurrentReserveStatusChanged += OutSlotDevice_CurrentReserveStatusChanged;
        }

        public int Index { get; }

        /// <summary>
        ///     Connection status of virtual output controller
        /// </summary>
        public IOutSlotDevice.AttachedStatus CurrentAttachedStatus { get; private set; }

        /// <summary>
        ///     Reference to output controller
        /// </summary>
        public IOutDevice OutputDevice { get; private set; }

        /// <summary>
        ///     Flag stating the connection preference of an output controller
        /// </summary>
        public IOutSlotDevice.ReserveStatus CurrentReserveStatus
        {
            get => reserveStatus;
            set
            {
                if (reserveStatus == value) return;
                reserveStatus = value;
                CurrentReserveStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Whether an input controller is associated with the slot
        /// </summary>
        public IOutSlotDevice.InputBound CurrentInputBound
        {
            get => inputBound;
            set
            {
                if (inputBound == value) return;
                inputBound = value;
                CurrentInputBoundChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Desired device type for a permanently connected slot
        /// </summary>
        public OutputDeviceType PermanentType
        {
            get => permanentType;
            set
            {
                if (permanentType == value) return;

                permanentType = value;
                PermanentTypeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Device type of the current output controller
        /// </summary>
        public OutputDeviceType CurrentType { get; set; }

        public event EventHandler CurrentReserveStatusChanged;
        public event EventHandler CurrentInputBoundChanged;
        public event EventHandler PermanentTypeChanged;

        private void OutSlotDevice_CurrentReserveStatusChanged(object sender, EventArgs e)
        {
            if (reserveStatus == IOutSlotDevice.ReserveStatus.Dynamic)
                PermanentType = OutputDeviceType.None;
            else if (CurrentType != OutputDeviceType.None) PermanentType = CurrentType;
        }

        public void AttachedDevice(IOutDevice outputDevice, OutputDeviceType contType)
        {
            OutputDevice = outputDevice;
            CurrentAttachedStatus = IOutSlotDevice.AttachedStatus.Attached;
            CurrentType = contType;
            //desiredType = contType;
        }

        public void DetachDevice()
        {
            if (OutputDevice != null)
            {
                OutputDevice = null;
                CurrentAttachedStatus = IOutSlotDevice.AttachedStatus.UnAttached;
                CurrentType = OutputDeviceType.None;
                CurrentInputBound = IOutSlotDevice.InputBound.Unbound;
                if (reserveStatus == IOutSlotDevice.ReserveStatus.Dynamic) PermanentType = OutputDeviceType.None;
            }
        }

        ~OutSlotDevice()
        {
            DetachDevice();
        }
    }
}

