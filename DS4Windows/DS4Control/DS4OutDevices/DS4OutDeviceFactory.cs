using System;
using DS4Windows.Shared.Emulator.ViGEmGen1.Types.Legacy;
using Nefarius.ViGEm.Client;

namespace DS4Windows
{
    internal static class DS4OutDeviceFactory
    {
        private static readonly Version ExtApiMinVersion = new("1.17.333.0");

        public static DS4OutDevice CreateDS4Device(ViGEmClient client,
            Version driverVersion)
        {
            DS4OutDevice result = null;
            if (ExtApiMinVersion.CompareTo(driverVersion) <= 0)
                result = new DS4OutDeviceExt(client);
            else
                result = new DS4OutDeviceBasic(client);

            return result;
        }
    }
}