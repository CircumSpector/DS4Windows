using System.Diagnostics.CodeAnalysis;

using MessagePipe;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nefarius.Utilities.HID.Devices;
using Nefarius.Utilities.HID.Util;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Configuration.Messages;

namespace Vapour.Shared.Devices.Services.Reporting;

internal sealed class OutputDeviceProcessor : IOutputDeviceProcessor
{
    private readonly IAsyncSubscriber<string, InputSourceFinalReport> _inputReportSubscriber;
    private IDisposable _inputReportSubscription;
    private IOutDevice _controllerDevice;

    public OutputDeviceProcessor(IServiceProvider serviceProvider,
        IAsyncSubscriber<string, InputSourceFinalReport> inputReportSubscriber)
    {
        _inputReportSubscriber = inputReportSubscriber;
        Services = serviceProvider;
        Logger = Services.GetRequiredService<ILogger<OutputDeviceProcessor>>();
    }

    private IServiceProvider Services { get; }
    private ILogger<OutputDeviceProcessor> Logger { get; }
    public IInputSource InputSource { get; private set; }
    private string _inputReportAvailableKey;

    public void StartOutputProcessing()
    {
        if (InputSource.Configuration.OutputDeviceType == OutputDeviceType.None)
        {
            return;
        }
        
        _inputReportSubscription = _inputReportSubscriber.Subscribe(_inputReportAvailableKey, InputReportAvailable);
            
        _controllerDevice = CreateControllerOutDevice();
        _controllerDevice.Connect();
    }

    public void StopOutputProcessing()
    {
        if (_controllerDevice == null)
        {
            return;
        }
        
        _inputReportSubscription.Dispose();
        _controllerDevice.Disconnect();
        _controllerDevice = null;
    }

    public void SetInputSource(IInputSource inputSource)
    {
        InputSource = inputSource;
        _inputReportAvailableKey = $"{InputSource.InputSourceKey}_{MessageKeys.InputReportAvailableKey}";
    }

    private ValueTask InputReportAvailable(InputSourceFinalReport report, CancellationToken cs)
    {
        if (InputSource.Configuration.OutputDeviceType == OutputDeviceType.None || _controllerDevice == null)
        {
            return ValueTask.CompletedTask;
        }

        report = UpdateBasedOnConfiguration(InputSource.Configuration, report);
        _controllerDevice.ConvertAndSendReport(report);

        return ValueTask.CompletedTask;
    }

    private InputSourceFinalReport UpdateBasedOnConfiguration(InputSourceConfiguration configuration,
        InputSourceFinalReport report)
    {
        CheckAndScaleIfNeeded(report);
        if (configuration.IsPassthru)
        {
            return report;
        }

        //TODO: fill in processing the configuration against the current report
        Guid profileId = configuration.ProfileId;
        return report;
    }

    private IOutDevice CreateControllerOutDevice()
    {
        IOutDevice outDevice;
        if (InputSource.Configuration.OutputDeviceType == OutputDeviceType.Xbox360Controller)
        {
            outDevice = Services.GetRequiredService<Xbox360OutDevice>();
        }
        else
        {
            outDevice = Services.GetRequiredService<DS4OutDevice>();
        }

        return outDevice;
    }

    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    private void CheckAndScaleIfNeeded(InputSourceFinalReport report)
    {
        switch (report.LThumbAxisScaleInputType)
        {
            case AxisRangeType.Short when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.Xbox360Controller:
                report.LeftThumbX = AxisScaling.ScaleDown(report.LeftThumbX, false);
                report.LeftThumbY = AxisScaling.ScaleDown(report.LeftThumbY, true);
                break;
            case AxisRangeType.Byte when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.DualShock4Controller:
                report.LeftThumbX = AxisScaling.ScaleUp(report.LeftThumbX, false);
                report.LeftThumbY = AxisScaling.ScaleUp(report.LeftThumbY, true);
                break;
        }

        switch (report.RThumbAxisScaleInputType)
        {
            case AxisRangeType.Short when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.Xbox360Controller:
                report.RightThumbX = AxisScaling.ScaleDown(report.RightThumbX, false);
                report.RightThumbY = AxisScaling.ScaleDown(report.RightThumbY, true);
                break;
            case AxisRangeType.Byte when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.DualShock4Controller:
                report.RightThumbX = AxisScaling.ScaleUp(report.RightThumbX, false);
                report.RightThumbY = AxisScaling.ScaleUp(report.RightThumbY, true);
                break;
        }
    }
}