using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nefarius.ViGEm.Client;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services.Reporting;

internal sealed class OutputDeviceProcessor : IOutputDeviceProcessor
{
    private const float RecipInputPosResolution = 1 / 127f;
    private const float RecipInputNegResolution = 1 / 128f;
    private const int OutputResolution = 32767 - -32768;
    private IOutDevice _controllerDevice;
    private IInputReportProcessor _inputReportProcessor;

    public OutputDeviceProcessor(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        Logger = Services.GetRequiredService<ILogger<OutputDeviceProcessor>>();
    }

    private IServiceProvider Services { get; }
    private ILogger<OutputDeviceProcessor> Logger { get; }
    public IInputSource InputSource { get; private set; }
    public event Action<OutputDeviceReport> OnOutputDeviceReportReceived;

    public void StartOutputProcessing(IInputReportProcessor inputReportProcessor)
    {
        if (InputSource.Configuration.OutputDeviceType == OutputDeviceType.None)
        {
            return;
        }

        _inputReportProcessor = inputReportProcessor;
        _inputReportProcessor.InputReportAvailable += InputReportAvailable;
        _controllerDevice = CreateControllerOutDevice();
        _controllerDevice.Connect();
    }

    public void StopOutputProcessing()
    {
        if (_controllerDevice == null)
        {
            return;
        }

        _controllerDevice.OnOutputDeviceReportReceived -= OutDevice_OnOutputDeviceReportReceived;
        _inputReportProcessor.InputReportAvailable -= InputReportAvailable;
        _controllerDevice.Disconnect();
        _controllerDevice = null;
    }

    public void SetInputSource(IInputSource inputSource)
    {
        InputSource = inputSource;
    }

    private void InputReportAvailable(IInputSource arg1,
        InputSourceFinalReport report)
    {
        if (InputSource.Configuration.OutputDeviceType == OutputDeviceType.None || _controllerDevice == null)
        {
            return;
        }

        report = UpdateBasedOnConfiguration(InputSource.Configuration, report);
        _controllerDevice.ConvertAndSendReport(report);
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
        ViGEmClient client = Services.GetRequiredService<ViGEmClient>();
        if (InputSource.Configuration.OutputDeviceType == OutputDeviceType.Xbox360Controller)
        {
            outDevice = new Xbox360OutDevice(client);
        }
        else
        {
            outDevice = new DS4OutDevice(client);
        }

        outDevice.OnOutputDeviceReportReceived += OutDevice_OnOutputDeviceReportReceived;

        return outDevice;
    }

    private void OutDevice_OnOutputDeviceReportReceived(OutputDeviceReport outputReport)
    {
        OnOutputDeviceReportReceived?.Invoke(outputReport);
    }

    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    private void CheckAndScaleIfNeeded(InputSourceFinalReport report)
    {
        switch (report.LThumbAxisScaleInputType)
        {
            case InputAxisType.Xbox when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.Xbox360Controller:
                report.LeftThumbX = ScaleDown(report.LeftThumbX, false);
                report.LeftThumbY = ScaleDown(report.LeftThumbY, true);
                break;
            case InputAxisType.DualShock4 when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.DualShock4Controller:
                report.LeftThumbX = ScaleUp(report.LeftThumbX, false);
                report.LeftThumbY = ScaleUp(report.LeftThumbY, true);
                break;
        }

        switch (report.RThumbAxisScaleInputType)
        {
            case InputAxisType.Xbox when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.Xbox360Controller:
                report.RightThumbX = ScaleDown(report.RightThumbX, false);
                report.RightThumbY = ScaleDown(report.RightThumbY, true);
                break;
            case InputAxisType.DualShock4 when
                InputSource.Configuration.OutputDeviceType != OutputDeviceType.DualShock4Controller:
                report.RightThumbX = ScaleUp(report.RightThumbX, false);
                report.RightThumbY = ScaleUp(report.RightThumbY, true);
                break;
        }
    }

    private static byte ScaleDown(short value, bool flip)
    {
        unchecked
        {
            byte newValue = (byte)((value + 0x8000) / 257);
            if (flip)
            {
                newValue = (byte)(byte.MaxValue - newValue);
            }

            return newValue;
        }
    }

    private static short ScaleUp(int value, bool flip)
    {
        unchecked
        {
            value -= 0x80;
            float recipRun = value >= 0 ? RecipInputPosResolution : RecipInputNegResolution;

            float temp = value * recipRun;
            //if (Flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
            if (flip)
            {
                temp = -temp;
            }

            temp = (temp + 1.0f) * 0.5f;

            return (short)((temp * OutputResolution) + -32768);
        }
    }
}