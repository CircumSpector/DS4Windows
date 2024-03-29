﻿using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Represents a device that can emit supported input reports and supports features like reading unique addresses
///     (serial numbers) etc.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
[SuppressMessage("ReSharper", "UnusedParameter.Global")]
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface ICompatibleHidDevice : IDisposable
{
    /// <summary>
    ///     The backing <see cref="IHidDevice" /> of this <see cref="ICompatibleHidDevice" />.
    /// </summary>
    IHidDevice SourceDevice { get; }

    /// <summary>
    ///     Gets the <see cref="ConnectionType" />.
    /// </summary>
    ConnectionType? Connection { get; }
    
    /// <summary>
    ///     The serial number (MAC address) of this <see cref="ICompatibleHidDevice" />.
    /// </summary>
    PhysicalAddress Serial { get; }

    /// <summary>
    ///     Whether or not the device is a filtered device
    /// </summary>
    bool IsFiltered { get; set; }

    /// <summary>
    ///     The parsed input report.
    /// </summary>
    IInputSourceReport InputSourceReport { get; }

    InputSourceConfiguration CurrentConfiguration { get; }

    IReadOnlyList<DeviceInfo> KnownDevices { get; }

    /// <summary>
    ///     The <see cref="Serial" /> as string.
    /// </summary>
    string SerialString { get; }

    string DeviceKey { get; }
    
    DeviceInfo CurrentDeviceInfo { get; }

    int Index { get; set; }

    MultiControllerConfigurationType MultiControllerConfigurationType { get; }

    /// <summary>
    ///     Fired when this device has been disconnected/unplugged.
    /// </summary>
    event Action<ICompatibleHidDevice> Disconnected;

    /// <summary>
    ///     Performs post-device-creation initialization tasks.
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Performs tasks to tear down this device.
    /// </summary>
    void FireDisconnected();

    /// <summary>
    ///     Transforms the input report byte array into a managed object.
    /// </summary>
    /// <param name="input"></param>
    void ProcessInputReport(ReadOnlySpan<byte> input);

    void SetConfiguration(InputSourceConfiguration profile);

    void OnAfterStartListening();
    
    int ReadInputReport(Span<byte> buffer);

    void RefreshConfiguration();

    Task<byte[]> ReadOutputReport(CancellationToken cancellationToken);

    void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport);
    Task DisconnectBTController();
    void Setup(IHidDevice hidDevice, DeviceInfo deviceInfo);
    void Close();
    void SetPlayerLedAndColor();
}