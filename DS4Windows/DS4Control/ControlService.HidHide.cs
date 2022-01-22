using System;
using System.IO;
using System.Text;
using DS4WinWPF.DS4Control;
using static DS4Windows.Global;

namespace DS4Windows
{
    public partial class ControlService
    {
        [Obsolete]
        public void CheckHidHidePresence()
        {
            if (hidHideInstalled)
            {
                LogDebug("HidHide control device found");
                using (var hidHideDevice = new HidHideAPIDevice())
                {
                    if (!hidHideDevice.IsOpen()) return;

                    var dosPaths = hidHideDevice.GetWhitelist();

                    var maxPathCheckLength = 512;
                    var sb = new StringBuilder(maxPathCheckLength);
                    var driveLetter = Path.GetPathRoot(ExecutableLocation).Replace("\\", "");
                    var _ = NativeMethods.QueryDosDevice(driveLetter, sb, maxPathCheckLength);
                    //int error = Marshal.GetLastWin32Error();

                    var dosDrivePath = sb.ToString();
                    // Strip a possible \??\ prefix.
                    if (dosDrivePath.StartsWith(@"\??\")) dosDrivePath = dosDrivePath.Remove(0, 4);

                    var partial = ExecutableLocation.Replace(driveLetter, "");
                    // Need to trim starting '\\' from path2 or Path.Combine will
                    // treat it as an absolute path and only return path2
                    var realPath = Path.Combine(dosDrivePath, partial.TrimStart('\\'));
                    var exists = dosPaths.Contains(realPath);
                    if (!exists)
                    {
                        LogDebug("DS4Windows not found in HidHide whitelist. Adding DS4Windows to list");
                        dosPaths.Add(realPath);
                        hidHideDevice.SetWhitelist(dosPaths);
                    }
                }
            }
        }

        [Obsolete]
        public void UpdateHidHideAttributes()
        {
            if (hidHideInstalled)
            {
                hidDeviceHidingAffectedDevs.Clear();
                hidDeviceHidingExemptedDevs.Clear(); // No known equivalent in HidHide
                hidDeviceHidingForced = false; // No known equivalent in HidHide
                hidDeviceHidingEnabled = false;

                using (var hidHideDevice = new HidHideAPIDevice())
                {
                    if (!hidHideDevice.IsOpen()) return;

                    var active = hidHideDevice.GetActiveState();
                    var instances = hidHideDevice.GetBlacklist();

                    hidDeviceHidingEnabled = active;
                    foreach (var instance in instances) hidDeviceHidingAffectedDevs.Add(instance.ToUpper());
                }
            }
        }

        [Obsolete]
        public void UpdateHidHiddenAttributes()
        {
            if (hidHideInstalled) UpdateHidHideAttributes();
        }

        [Obsolete]
        private bool CheckAffected(DS4Device dev)
        {
            var result = false;
            if (dev != null && hidDeviceHidingEnabled)
            {
                var deviceInstanceId = DS4DeviceEnumerator.DevicePathToInstanceId(dev.HidDevice.DevicePath);
                if (hidHideInstalled)
                    result = CheckHidHideAffectedStatus(deviceInstanceId,
                        hidDeviceHidingAffectedDevs, hidDeviceHidingExemptedDevs, hidDeviceHidingForced);
            }

            return result;
        }
    }
}
