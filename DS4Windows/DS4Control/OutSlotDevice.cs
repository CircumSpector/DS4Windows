using System;
using DS4Windows;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Emulator.ViGEmGen1.Types.Legacy;
using DS4WinWPF.DS4Control.Logging;

namespace DS4WinWPF.DS4Control
{
    public class OutSlotDevice
    {
        public enum AttachedStatus : uint
        {
            UnAttached = 0,
            Attached = 1
        }

        public enum InputBound : uint
        {
            Unbound = 0,
            Bound = 1
        }

        public enum ReserveStatus : uint
        {
            Dynamic = 0,
            Permanent = 1
        }

        private InputBound inputBound;
        private OutputDeviceType permanentType;
        private ReserveStatus reserveStatus;

        public OutSlotDevice(int idx)
        {
            Index = idx;
            CurrentReserveStatusChanged += OutSlotDevice_CurrentReserveStatusChanged;
        }

        public int Index { get; }

        /// <summary>
        ///     Connection status of virtual output controller
        /// </summary>
        public AttachedStatus CurrentAttachedStatus { get; private set; }

        /// <summary>
        ///     Reference to output controller
        /// </summary>
        public OutputDevice OutputDevice { get; private set; }

        /// <summary>
        ///     Flag stating the connection preference of an output controller
        /// </summary>
        public ReserveStatus CurrentReserveStatus
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
        public InputBound CurrentInputBound
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

                if (value != OutputDeviceType.None)
                    AppLogger.Instance.LogToGui($"Output slot #{Index + 1} has permanent type {value}", false);

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
            if (reserveStatus == ReserveStatus.Dynamic)
                PermanentType = OutputDeviceType.None;
            else if (CurrentType != OutputDeviceType.None) PermanentType = CurrentType;
        }

        public void AttachedDevice(OutputDevice outputDevice, OutputDeviceType contType)
        {
            OutputDevice = outputDevice;
            CurrentAttachedStatus = AttachedStatus.Attached;
            CurrentType = contType;
            //desiredType = contType;
        }

        public void DetachDevice()
        {
            if (OutputDevice != null)
            {
                OutputDevice = null;
                CurrentAttachedStatus = AttachedStatus.UnAttached;
                CurrentType = OutputDeviceType.None;
                CurrentInputBound = InputBound.Unbound;
                if (reserveStatus == ReserveStatus.Dynamic) PermanentType = OutputDeviceType.None;
            }
        }

        ~OutSlotDevice()
        {
            DetachDevice();
        }
    }
}