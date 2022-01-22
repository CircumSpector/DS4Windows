using System;
using System.Collections.Generic;
using System.Windows;
using DS4Windows;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Control;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class CurrentOutDeviceViewModel
    {
        private readonly ControlService controlService;

        private readonly IOutputSlotManager outSlotManager;
        private int selectedIndex = -1;

        public CurrentOutDeviceViewModel(ControlService controlService,
            IOutputSlotManager outputMan)
        {
            outSlotManager = outputMan;
            // Set initial capacity at input controller limit in app
            SlotDeviceEntries = new List<SlotDeviceEntry>(ControlService.CURRENT_DS4_CONTROLLER_LIMIT);
            var idx = 0;
            foreach (var tempDev in outputMan.OutputSlots)
            {
                var tempEntry = new SlotDeviceEntry(tempDev, idx);
                tempEntry.PluginRequest += OutSlot_PluginRequest;
                tempEntry.UnplugRequest += OutSlot_UnplugRequest;
                SlotDeviceEntries.Add(tempEntry);
                idx++;
            }

            this.controlService = controlService;

            outSlotManager.SlotAssigned += OutSlotManager_SlotAssigned;
            outSlotManager.SlotUnassigned += OutSlotManager_SlotUnassigned;
            SelectedIndexChanged += CurrentOutDeviceViewModel_SelectedIndexChanged;
        }

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Visibility SidePanelVisibility
        {
            get
            {
                var result = Visibility.Collapsed;
                if (selectedIndex >= 0)
                {
                    var temp = SlotDeviceEntries[selectedIndex];
                    if (temp.OutSlotDevice.CurrentType != OutputDeviceType.None) result = Visibility.Visible;
                }

                return result;
            }
        }

        public bool PluginEnabled
        {
            get
            {
                var result = false;
                if (selectedIndex >= 0)
                {
                    var temp = SlotDeviceEntries[selectedIndex];
                    if (temp.OutSlotDevice.CurrentAttachedStatus ==
                        OutSlotDevice.AttachedStatus.UnAttached)
                        result = true;
                }

                return result;
            }
        }

        public bool UnpluginEnabled
        {
            get
            {
                var result = false;
                if (selectedIndex >= 0)
                {
                    var temp = SlotDeviceEntries[selectedIndex];
                    if (temp.OutSlotDevice.CurrentAttachedStatus ==
                        OutSlotDevice.AttachedStatus.Attached)
                        result = true;
                }

                return result;
            }
        }

        public List<SlotDeviceEntry> SlotDeviceEntries { get; }

        public event EventHandler SelectedIndexChanged;
        public event EventHandler SidePanelVisibilityChanged;
        public event EventHandler PluginEnabledChanged;
        public event EventHandler UnpluginEnabledChanged;

        private void CurrentOutDeviceViewModel_SelectedIndexChanged(object sender,
            EventArgs e)
        {
            SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            PluginEnabledChanged?.Invoke(this, EventArgs.Empty);
            UnpluginEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutSlot_PluginRequest(object sender, EventArgs e)
        {
            var entry = sender as SlotDeviceEntry;
            if (entry.OutSlotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached &&
                entry.OutSlotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
                controlService.EventDispatcher.BeginInvoke((Action)(() =>
                {
                    controlService.AttachUnboundOutDev(entry.OutSlotDevice, entry.OutSlotDevice.CurrentType);
                    //SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                }));
        }

        private void OutSlot_UnplugRequest(object sender, EventArgs e)
        {
            var entry = sender as SlotDeviceEntry;
            if (entry.OutSlotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached &&
                entry.OutSlotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
                controlService.EventDispatcher.BeginInvoke((Action)(() =>
                {
                    controlService.DetachUnboundOutDev(entry.OutSlotDevice);
                    //SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                }));
        }

        private void OutSlotManager_SlotUnassigned(OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            SlotDeviceEntries[slotNum].RemovedDevice();
            RefreshPanels();
        }

        private void OutSlotManager_SlotAssigned(OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            SlotDeviceEntries[slotNum].AssignedDevice();
            RefreshPanels();
        }

        private void RefreshPanels()
        {
            SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            PluginEnabledChanged?.Invoke(this, EventArgs.Empty);
            UnpluginEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SlotDeviceEntry
    {
        private int desiredTypeChoiceIndex = -1;

        private bool dirty;
        private int idx;

        private int reserveChoiceIndex = -1;

        public SlotDeviceEntry(OutSlotDevice outSlotDevice, int idx)
        {
            OutSlotDevice = outSlotDevice;
            this.idx = idx;

            //desiredTypeChoiceIndex = DetermineDesiredChoiceIdx();
            reserveChoiceIndex = DetermineReserveChoiceIdx();

            SetupEvents();
        }

        public OutSlotDevice OutSlotDevice { get; }

        public string CurrentType
        {
            get
            {
                var temp = "Empty";
                if (OutSlotDevice.OutputDevice != null) temp = OutSlotDevice.OutputDevice.GetDeviceType();

                return temp;
            }
        }

        public string DesiredType
        {
            get
            {
                var temp = "Dynamic";
                if (OutSlotDevice.CurrentReserveStatus ==
                    OutSlotDevice.ReserveStatus.Permanent)
                    temp = OutSlotDevice.PermanentType.ToString();

                return temp;
            }
        }

        public bool BoundInput => OutSlotDevice.CurrentInputBound == OutSlotDevice.InputBound.Bound;

        public int DesiredTypeChoice
        {
            get => desiredTypeChoiceIndex;
            set
            {
                if (desiredTypeChoiceIndex == value) return;
                desiredTypeChoiceIndex = value;
                DesiredTypeChoiceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ReserveChoice
        {
            get => reserveChoiceIndex;
            set
            {
                if (reserveChoiceIndex == value) return;
                reserveChoiceIndex = value;
                ReserveChoiceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Dirty
        {
            get => dirty;
            set
            {
                if (dirty == value) return;
                dirty = value;
                DirtyChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CurrentTypeChanged;
        public event EventHandler DesiredTypeChanged;
        public event EventHandler BoundInputChanged;
        public event EventHandler DesiredTypeChoiceChanged;
        public event EventHandler ReserveChoiceChanged;
        public event EventHandler DirtyChanged;

        public event EventHandler PluginRequest;
        public event EventHandler UnplugRequest;

        private void SetupEvents()
        {
            //DesiredTypeChoiceChanged += SlotDeviceEntry_FormPropChanged;
            ReserveChoiceChanged += SlotDeviceEntry_FormPropChanged;

            OutSlotDevice.PermanentTypeChanged += OutSlotDevice_PermanentTypeChanged;
            OutSlotDevice.CurrentInputBoundChanged += OutSlotDevice_CurrentInputBoundChanged;
        }

        private void OutSlotDevice_CurrentInputBoundChanged(object sender, EventArgs e)
        {
            BoundInputChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutSlotDevice_PermanentTypeChanged(object sender, EventArgs e)
        {
            DesiredTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SlotDeviceEntry_FormPropChanged(object sender, EventArgs e)
        {
            Dirty = true;
        }

        private int DetermineDesiredChoiceIdx()
        {
            var result = 0;
            switch (OutSlotDevice.PermanentType)
            {
                case OutputDeviceType.None:
                    result = 0;
                    break;
                case OutputDeviceType.Xbox360Controller:
                    result = 1;
                    break;
                case OutputDeviceType.DualShock4Controller:
                    result = 2;
                    break;
            }

            return result;
        }

        private int DetermineReserveChoiceIdx()
        {
            var result = 0;
            switch (OutSlotDevice.CurrentReserveStatus)
            {
                case OutSlotDevice.ReserveStatus.Dynamic:
                    result = 0;
                    break;
                case OutSlotDevice.ReserveStatus.Permanent:
                    result = 1;
                    break;
            }

            return result;
        }

        private OutputDeviceType DetermineDesiredTypeFromIdx()
        {
            var result = OutputDeviceType.None;
            switch (desiredTypeChoiceIndex)
            {
                case 0:
                    result = OutputDeviceType.None;
                    break;
                case 1:
                    result = OutputDeviceType.Xbox360Controller;
                    break;
                case 2:
                    result = OutputDeviceType.DualShock4Controller;
                    break;
            }

            return result;
        }

        private OutSlotDevice.ReserveStatus DetermineReserveChoiceFromIdx()
        {
            var result = OutSlotDevice.ReserveStatus.Dynamic;
            switch (reserveChoiceIndex)
            {
                case 0:
                    result = OutSlotDevice.ReserveStatus.Dynamic;
                    break;
                case 1:
                    result = OutSlotDevice.ReserveStatus.Permanent;
                    break;
            }

            return result;
        }

        public void AssignedDevice()
        {
            Refresh();
        }

        public void RemovedDevice()
        {
            Refresh();
        }

        private void Refresh()
        {
            //DesiredTypeChoice = DetermineDesiredChoiceIdx();
            ReserveChoice = DetermineReserveChoiceIdx();

            CurrentTypeChanged?.Invoke(this, EventArgs.Empty);
            DesiredTypeChanged?.Invoke(this, EventArgs.Empty);
            BoundInputChanged?.Invoke(this, EventArgs.Empty);
            Dirty = false;
        }

        public void RequestPlugin()
        {
            PluginRequest?.Invoke(this, EventArgs.Empty);
        }

        public void RequestUnplug()
        {
            UnplugRequest?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyChanges()
        {
            OutSlotDevice.CurrentReserveStatus = DetermineReserveChoiceFromIdx();
            /*if (outSlotDevice.CurrentReserveStatus ==
                OutSlotDevice.ReserveStatus.Permanent)
            {
                outSlotDevice.PermanentType = outSlotDevice.CurrentType;
            }
            else
            {
                outSlotDevice.PermanentType = OutContType.None;
            }
            */
        }
    }
}