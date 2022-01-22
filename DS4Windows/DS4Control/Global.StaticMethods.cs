using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DS4Windows.DS4Control;
using DS4Windows.Shared.Common.Converters;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Legacy;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows
{
    public partial class Global
    {
        public static ulong CompileVersionNumberFromString(string versionStr)
        {
            ulong result = 0;
            try
            {
                var tmpVersion = new Version(versionStr);
                result = CompileVersionNumber(tmpVersion.Major, tmpVersion.Minor,
                    tmpVersion.Build, tmpVersion.Revision);
            }
            catch (Exception)
            {
            }

            return result;
        }

        public static ulong CompileVersionNumber(int majorPart, int minorPart,
            int buildPart, int privatePart)
        {
            var result = ((ulong)majorPart << 48) | ((ulong)minorPart << 32) |
                         ((ulong)buildPart << 16) | (ushort)privatePart;
            return result;
        }

        public static bool CheckForDevice(string guid)
        {
            var result = false;
            var deviceGuid = Guid.Parse(guid);
            var deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                Marshal.SizeOf(deviceInfoData);

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref deviceGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            result = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        private static bool CheckForSysDevice(string searchHardwareId)
        {
            var result = false;
            var sysGuid = Constants.SystemDeviceClassGuid;
            var deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (var i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                        ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                        dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    var hardwareId = dataBuffer.ToUTF16String();
                    //if (hardwareIds.Contains("Virtual Gamepad Emulation Bus"))
                    //    result = true;
                    if (hardwareId.Equals(searchHardwareId))
                        result = true;
                }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        internal static string GetDeviceProperty(string deviceInstanceId,
            NativeMethods.DEVPROPKEY prop)
        {
            var result = string.Empty;
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;

            var hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidGuid, deviceInstanceId, 0,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);
            NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                result = dataBuffer.ToUTF16String();

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        public static bool CheckHidHideAffectedStatus(string deviceInstanceId,
            HashSet<string> affectedDevs, HashSet<string> exemptedDevices, bool force = false)
        {
            var result = false;
            var tempDeviceInstanceId = deviceInstanceId.ToUpper();
            result = affectedDevs.Contains(tempDeviceInstanceId);
            return result;
        }

        public static bool IsViGEmBusInstalled()
        {
            return IsViGEmInstalled;
        }

        public static void RefreshHidHideInfo()
        {
            hidHideInstalled = IsHidHideInstalled;
        }

        private static void PopulateFromViGEmBusInfo(ViGEmBusInfo busInfo)
        {
            if (busInfo != null)
            {
                IsViGEmInstalled = true;
                ViGEmBusVersion = busInfo.deviceVersionStr;
            }
            else
            {
                IsViGEmInstalled = false;
                ViGEmBusVersion = BLANK_VIGEMBUS_VERSION;
            }
        }

        private static string FakerInputVersion()
        {
            // Start with BLANK_FAKERINPUT_VERSION for result
            var result = BLANK_FAKERINPUT_VERSION;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref Util.fakerInputGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            var foundDev = false;
            //bool success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            for (var i = 0; !foundDev && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                ulong devPropertyType = 0;
                var requiredSizeProp = 0;
                NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                    ref NativeMethods.DEVPKEY_Device_DriverVersion, ref devPropertyType, null, 0, ref requiredSizeProp,
                    0);

                if (requiredSizeProp > 0)
                {
                    var versionTextBuffer = new byte[requiredSizeProp];
                    NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                        ref NativeMethods.DEVPKEY_Device_DriverVersion, ref devPropertyType, versionTextBuffer,
                        requiredSizeProp, ref requiredSizeProp, 0);

                    var tmpitnow = Encoding.Unicode.GetString(versionTextBuffer);
                    var tempStrip = tmpitnow.TrimEnd('\0');
                    foundDev = true;
                    result = tempStrip;
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        public static string GetX360ControlString(X360ControlItem key, OutputDeviceType conType)
        {
            var result = string.Empty;

            switch (conType)
            {
                case OutputDeviceType.Xbox360Controller:
                    result = EnumDescriptionConverter.GetEnumDescription(key);
                    break;
                case OutputDeviceType.DualShock4Controller:
                    Ds4DefaultNames.TryGetValue(key, out result);
                    break;
            }

            return result;
        }

        public static double Clamp(double min, double value, double max)
        {
            return value < min ? min : value > max ? max : value;
        }

        private static int ClampInt(int min, int value, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static void InitOutputKBMHandler(string identifier)
        {
            outputKBMHandler = VirtualKBMFactory.DetermineHandler(identifier);
        }

        public static void InitOutputKBMMapping(string identifier)
        {
            outputKBMMapping = VirtualKBMFactory.GetMappingInstance(identifier);
        }

        public static void RefreshFakerInputInfo()
        {
            fakerInputInstalled = IsFakerInputInstalled;
            fakerInputVersion = FakerInputVersion();
        }

        /// <summary>
        ///     Take Windows virtual key value and refresh action alias for currently used output KB+M system
        /// </summary>
        /// <param name="setting">Instance of edited DS4ControlSettings object</param>
        /// <param name="shift">Flag to indicate if shift action is being modified</param>
        public static void RefreshActionAlias(DS4ControlSettingsV3 setting, bool shift)
        {
            if (!shift)
            {
                setting.ActionData.ActionAlias = 0;
                if (setting.ControlActionType == DS4ControlSettingsV3.ActionType.Key)
                    setting.ActionData.ActionAlias =
                        outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ActionData.ActionKey));
            }
            else
            {
                setting.ShiftAction.ActionAlias = 0;
                if (setting.ShiftActionType == DS4ControlSettingsV3.ActionType.Key)
                    setting.ShiftAction.ActionAlias =
                        outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ShiftAction.ActionKey));
            }
        }
    }
}