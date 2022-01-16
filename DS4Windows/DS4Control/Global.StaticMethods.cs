using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using DS4Windows.DS4Control;

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

        private static void FindViGEmDeviceInfo()
        {
            var result = false;
            var deviceGuid = Constants.ViGemBusGen1InterfaceGuid;
            var deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                Marshal.SizeOf(deviceInfoData);

            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;

            // Properties to retrieve
            NativeMethods.DEVPROPKEY[] lookupProperties =
            {
                NativeMethods.DEVPKEY_Device_DriverVersion, NativeMethods.DEVPKEY_Device_InstanceId,
                NativeMethods.DEVPKEY_Device_Manufacturer, NativeMethods.DEVPKEY_Device_Provider,
                NativeMethods.DEVPKEY_Device_DeviceDesc
            };

            var tempViGEmBusInfoList = new List<ViGEmBusInfo>();

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref deviceGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            for (var i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                var tempBusInfo = new ViGEmBusInfo();

                foreach (var currentDevKey in lookupProperties)
                {
                    var tempKey = currentDevKey;
                    if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                        ref tempKey, ref propertyType,
                        dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                    {
                        var temp = dataBuffer.ToUTF16String();
                        if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DriverVersion.fmtid &&
                            currentDevKey.pid == NativeMethods.DEVPKEY_Device_DriverVersion.pid)
                            try
                            {
                                tempBusInfo.deviceVersion = new Version(temp);
                                tempBusInfo.deviceVersionStr = temp;
                            }
                            catch (ArgumentException)
                            {
                                // Default to unknown version
                                tempBusInfo.deviceVersionStr = BLANK_VIGEMBUS_VERSION;
                                tempBusInfo.deviceVersion = new Version(tempBusInfo.deviceVersionStr);
                            }
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_InstanceId.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_InstanceId.pid)
                            tempBusInfo.instanceId = temp;
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Manufacturer.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_Manufacturer.pid)
                            tempBusInfo.manufacturer = temp;
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Provider.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_Provider.pid)
                            tempBusInfo.driverProviderName = temp;
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DeviceDesc.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_DeviceDesc.pid)
                            tempBusInfo.deviceName = temp;
                    }
                }

                tempViGEmBusInfoList.Add(tempBusInfo);
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            // Iterate over list and find most recent version number
            //IEnumerable<ViGEmBusInfo> tempResults = tempViGEmBusInfoList.Where(item => MinimumSupportedViGEmBusVersionInfo.CompareTo(item.deviceVersion) <= 0);
            var latestKnown = new Version(BLANK_VIGEMBUS_VERSION);
            var deviceInstanceId = string.Empty;
            foreach (var item in tempViGEmBusInfoList)
                if (latestKnown.CompareTo(item.deviceVersion) <= 0)
                {
                    latestKnown = item.deviceVersion;
                    deviceInstanceId = item.instanceId;
                }

            // Get bus info for most recent version found and save info
            var latestBusInfo =
                tempViGEmBusInfoList.SingleOrDefault(item => item.instanceId == deviceInstanceId);
            PopulateFromViGEmBusInfo(latestBusInfo);
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

        public static void RefreshViGEmBusInfo()
        {
            FindViGEmDeviceInfo();
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

        public static string GetX360ControlString(X360Controls key, OutContType conType)
        {
            var result = string.Empty;

            switch (conType)
            {
                case OutContType.X360:
                    XboxDefaultNames.TryGetValue(key, out result);
                    break;
                case OutContType.DS4:
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