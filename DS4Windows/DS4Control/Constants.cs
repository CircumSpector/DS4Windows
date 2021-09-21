using System;

namespace DS4Windows
{
    /// <summary>
    ///     Constant value definitions used throughout the solution.
    /// </summary>
    internal static class Constants
    {
        public const string ApplicationName = "DS4Windows";

        public const string ProfilesFileName = "Profiles.xml";
        
        public const string ActionsFileName = "Actions.xml";

        public const string LinkedProfilesFileName = "LinkedProfiles.xml";

        public const string ControllerConfigsFileName = "ControllerConfigs.xml";

        public const string AutoProfilesFileName = "Auto Profiles.xml";

        public static Guid ViGemBusInterfaceGuid = new("{96E42B22-F5E9-42F8-B043-ED0F932F014F}");

        public static Guid SystemDeviceClassGuid = new("{4d36e97d-e325-11ce-bfc1-08002be10318}");

        public static Guid WindowsScriptHostShellObjectGuild = new("{72C24DD5-D70A-438B-8A42-98424B88AFB8}");

        public const string ProfilesSubDirectory = "Profiles";

        public const string OutputSlotsFileName = "OutputSlots.xml";

        public const string SingleAppComEventName = "{a52b5b20-d9ee-4f32-8518-307fa14aa0c6}";

        public static Guid BluetoothHidGuild = new("{00001124-0000-1000-8000-00805F9B34FB}");
    }
}
