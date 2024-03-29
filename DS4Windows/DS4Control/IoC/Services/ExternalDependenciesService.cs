﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    internal class DeviceNodeInfo
    {
        public string DeviceName { get; init; }

        public Version DriverVersion { get; init; }

        public string DriverProviderName { get; init; }

        public string InstanceId { get; init; }

        public string Manufacturer { get; init; }
    }

    internal interface IExternalDependenciesService
    {
        IEnumerable<DeviceNodeInfo> ViGEmBusGen1Versions { get; }

        [CanBeNull] DeviceNodeInfo ViGEmBusGen1LatestVersion { get; }

        IEnumerable<DeviceNodeInfo> HidHideVersions { get; }

        [CanBeNull] DeviceNodeInfo HidHideLatestVersion { get; }
    }

    internal class ExternalDependenciesService : IExternalDependenciesService
    {
        public IEnumerable<DeviceNodeInfo> ViGEmBusGen1Versions =>
            GetDeviceInfoForInterfaceGuid(Constants.ViGemBusGen1InterfaceGuid);

        public DeviceNodeInfo ViGEmBusGen1LatestVersion =>
            ViGEmBusGen1Versions.OrderBy(v => v.DriverVersion).FirstOrDefault();

        public IEnumerable<DeviceNodeInfo> HidHideVersions =>
            GetDeviceInfoForInterfaceGuid(Constants.HidHideInterfaceGuid);

        public DeviceNodeInfo HidHideLatestVersion => HidHideVersions.OrderBy(v => v.DriverVersion).FirstOrDefault();

        private static IEnumerable<DeviceNodeInfo> GetDeviceInfoForInterfaceGuid(Guid deviceGuid)
        {
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
                NativeMethods.DEVPKEY_Device_DriverVersion,
                NativeMethods.DEVPKEY_Device_InstanceId,
                NativeMethods.DEVPKEY_Device_Manufacturer,
                NativeMethods.DEVPKEY_Device_Provider,
                NativeMethods.DEVPKEY_Device_DeviceDesc
            };

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(
                ref deviceGuid,
                null,
                0,
                NativeMethods.DIGCF_DEVICEINTERFACE
            );

            for (var i = 0;
                 NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData);
                 i++)
            {
                var version = new Version();
                var instanceId = string.Empty;
                var manufacturer = string.Empty;
                var providerName = string.Empty;
                var deviceName = string.Empty;

                foreach (var currentDevKey in lookupProperties)
                {
                    var nodeKey = currentDevKey;

                    if (!NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                            ref nodeKey, ref propertyType,
                            dataBuffer, dataBuffer.Length, ref requiredSize, 0)) continue;

                    var buffer = dataBuffer.ToUTF16String();

                    if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DriverVersion.fmtid &&
                        currentDevKey.pid == NativeMethods.DEVPKEY_Device_DriverVersion.pid)
                        try
                        {
                            version = new Version(buffer);
                        }
                        catch (ArgumentException)
                        {
                            version = new Version();
                        }
                    else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_InstanceId.fmtid &&
                             currentDevKey.pid == NativeMethods.DEVPKEY_Device_InstanceId.pid)
                        instanceId = buffer;
                    else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Manufacturer.fmtid &&
                             currentDevKey.pid == NativeMethods.DEVPKEY_Device_Manufacturer.pid)
                        manufacturer = buffer;
                    else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Provider.fmtid &&
                             currentDevKey.pid == NativeMethods.DEVPKEY_Device_Provider.pid)
                        providerName = buffer;
                    else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DeviceDesc.fmtid &&
                             currentDevKey.pid == NativeMethods.DEVPKEY_Device_DeviceDesc.pid)
                        deviceName = buffer;
                }

                yield return new DeviceNodeInfo
                {
                    DeviceName = deviceName,
                    DriverVersion = version,
                    DriverProviderName = providerName,
                    InstanceId = instanceId,
                    Manufacturer = manufacturer
                };
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }
    }
}