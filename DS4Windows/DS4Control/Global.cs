﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml;
using DS4Windows.DS4Control;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows
{
    public partial class Global
    {
        // Use 15 minutes for default Idle Disconnect when initially enabling the option
        public const int DEFAULT_ENABLE_IDLE_DISCONN_MINS = 15;
        public const int MAX_DS4_CONTROLLER_COUNT = 8;
        public const int TEST_PROFILE_ITEM_COUNT = MAX_DS4_CONTROLLER_COUNT + 1;
        public const int TEST_PROFILE_INDEX = TEST_PROFILE_ITEM_COUNT - 1;
        public const int OLD_XINPUT_CONTROLLER_COUNT = 4;
        public const byte DS4_STICK_AXIS_MIDPOINT = 128;

        private const string BLANK_VIGEMBUS_VERSION = "0.0.0.0";
        private const string MIN_SUPPORTED_VIGEMBUS_VERSION = "1.16.112.0";
        public const string BLANK_FAKERINPUT_VERSION = "0.0.0.0";

        public const int CONFIG_VERSION = 5;
        public const int APP_CONFIG_VERSION = 2;
        public const string ASSEMBLY_RESOURCE_PREFIX = "pack://application:,,,/DS4Windows;";
        public const string RESOURCES_PREFIX = "/DS4Windows;component/Resources";
        public const string CUSTOM_EXE_CONFIG_FILENAME = "custom_exe_name.txt";
        public const string XML_EXTENSION = ".xml";
        private static readonly Lazy<Global> LazyInstance = new(() => new Global());

        protected static int m_IdleTimeout = 600000;

        public static string[] TempProfileNames = new string[TEST_PROFILE_ITEM_COUNT]
        {
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, string.Empty
        };

        public static bool[] UseTempProfiles = new bool[TEST_PROFILE_ITEM_COUNT]
            { false, false, false, false, false, false, false, false, false };

        public static bool[] TempProfileDistance = new bool[TEST_PROFILE_ITEM_COUNT]
            { false, false, false, false, false, false, false, false, false };

        public static bool[] TouchpadActive = new bool[TEST_PROFILE_ITEM_COUNT]
            { true, true, true, true, true, true, true, true, true };


        public static bool hidHideInstalled = IsHidHideInstalled;
        public static bool fakerInputInstalled = IsFakerInputInstalled;
        public static string fakerInputVersion = FakerInputVersion();

        public static VirtualKBMBase outputKBMHandler;
        public static VirtualKBMMapping outputKBMMapping;

        public static Dictionary<TrayIconChoice, string> IconChoiceResources = new()
        {
            [TrayIconChoice.Default] = "/DS4Windows;component/Resources/Vapour.ico",
            [TrayIconChoice.Colored] = "/DS4Windows;component/Resources/Vapour.ico",
            [TrayIconChoice.White] = "/DS4Windows;component/Resources/DS4W - White.ico",
            [TrayIconChoice.Black] = "/DS4Windows;component/Resources/DS4W - Black.ico"
        };

        private readonly BackingStore _config = new();

        /// <summary>
        ///     Singleton instance of <see cref="Global" />.
        /// </summary>
        public static Global Instance => LazyInstance.Value;

        /// <summary>
        ///     Configuration data which gets persisted to disk.
        /// </summary>
        public IBackingStore Config => _config;

        public bool IsFirstRun { get; set; }

        public bool HasMultipleSaveSpots { get; set; }

        public bool RunHotPlug { get; set; } = false;

        public static bool IsViGEmInstalled { get; private set; }

        public static string ViGEmBusVersion { get; private set; } = BLANK_VIGEMBUS_VERSION;

        public static Version ViGEmBusVersionInfo => new(ViGEmBusVersion);

        private static Version MinimumSupportedViGEmBusVersionInfo => new(MIN_SUPPORTED_VIGEMBUS_VERSION);

        public static bool IsHidHideInstalled => CheckForSysDevice(@"root\HidHide");

        public static bool IsFakerInputInstalled => CheckForSysDevice(@"root\FakerInput");

        public static bool IsRunningSupportedViGEmBus => IsViGEmInstalled &&
                                                         MinimumSupportedViGEmBusVersionInfo.CompareTo(
                                                             ViGEmBusVersionInfo) <= 0;

        public string LastVersionChecked
        {
            get => _config.lastVersionChecked;
            set
            {
                _config.lastVersionChecked = value;
                _config.LastVersionCheckedNumber = CompileVersionNumberFromString(value);
            }
        }

        /// <summary>
        ///     Fake name used for user copy of DS4Windows.exe
        /// </summary>
        public string FakeExeName
        {
            get => _config.FakeExeFileName;
            set
            {
                var valid = !(value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
                if (valid) _config.FakeExeFileName = value;
            }
        }

        public bool[] TouchActive => TouchpadActive;

        public static bool CreateAutoProfiles(string m_Profile)
        {
            var Saved = true;

            try
            {
                XmlNode Node;
                var doc = new XmlDocument();

                Node = doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                doc.AppendChild(Node);

                Node = doc.CreateComment(string.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                doc.AppendChild(Node);

                Node = doc.CreateWhitespace("\r\n");
                doc.AppendChild(Node);

                Node = doc.CreateNode(XmlNodeType.Element, "Programs", "");
                doc.AppendChild(Node);
                doc.Save(m_Profile);
            }
            catch
            {
                Saved = false;
            }

            return Saved;
        }

        public static event EventHandler<EventArgs>
            ControllerStatusChange; // called when a controller is added/removed/battery or touchpad mode changes/etc.

        public static event EventHandler<SerialChangeArgs> DeviceSerialChange;

        public static void OnDeviceSerialChange(object sender, int index, PhysicalAddress serial)
        {
            if (DeviceSerialChange != null)
            {
                var args = new SerialChangeArgs(index, serial);
                DeviceSerialChange(sender, args);
            }
        }

        private class ViGEmBusInfo
        {
            public string deviceName;
            public Version deviceVersion;
            public string deviceVersionStr;

            public string driverProviderName;

            //public string path;
            public string instanceId;
            public string manufacturer;
        }
    }
}