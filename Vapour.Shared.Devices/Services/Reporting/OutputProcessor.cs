﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nefarius.ViGEm.Client;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services.Reporting;

internal sealed class OutputProcessor : IOutputProcessor
{
    private readonly IInputReportProcessor _inputReportProcessor;
    private IOutDevice _controllerDevice;

    private const float RecipInputPosResolution = 1 / 127f;
    private const float RecipInputNegResolution = 1 / 128f;
    private const int OutputResolution = 32767 - -32768;

    public OutputProcessor(IInputReportProcessor inputReportProcessor,
        IServiceProvider serviceProvider)
    {
        _inputReportProcessor = inputReportProcessor;

        Services = serviceProvider;
        Logger = Services.GetRequiredService<ILogger<OutputProcessor>>();
    }

    private IServiceProvider Services { get; }
    private ILogger<OutputProcessor> Logger { get; }
    public IInputSource InputSource { get; private set; }

    public void StartOutputProcessing()
    {
        if (InputSource.Configuration.OutputDeviceType != OutputDeviceType.None)
        {
            _inputReportProcessor.InputReportAvailable += _inputReportProcessor_InputReportAvailable;
            _controllerDevice = CreateControllerOutDevice();
            _controllerDevice.Connect();
        }
    }

    public void StopOutputProcessing()
    {
        if (_controllerDevice != null)
        {
            _inputReportProcessor.InputReportAvailable -= _inputReportProcessor_InputReportAvailable;
            _controllerDevice.Disconnect();
            _controllerDevice = null;
        }
    }

    public void SetDevice(IInputSource inputSource)
    {
        InputSource = inputSource;
    }

    private void _inputReportProcessor_InputReportAvailable(IInputSource arg1,
        InputSourceReport report)
    {
        if (InputSource.Configuration.OutputDeviceType != OutputDeviceType.None && _controllerDevice != null)
        {
            report = UpdateBasedOnConfiguration(InputSource.Configuration, report);
            _controllerDevice.ConvertAndSendReport(report);
        }
    }

    private InputSourceReport UpdateBasedOnConfiguration(InputSourceConfiguration configuration,
        InputSourceReport report)
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
        ViGEmClient client = Services.GetService<ViGEmClient>();
        if (InputSource.Configuration.OutputDeviceType == OutputDeviceType.Xbox360Controller)
        {
            outDevice = new Xbox360OutDevice(client);
        }
        else
        {
            outDevice = new DS4OutDevice(client);
        }

        return outDevice;
    }

    private void CheckAndScaleIfNeeded(InputSourceReport report)
    {
        if (report.AxisScaleInputType == InputAxisType.Xbox &&
            InputSource.Configuration.OutputDeviceType != OutputDeviceType.Xbox360Controller)
        {
            report.LeftThumbX = ScaleDown(report.LeftThumbX, false);
            report.LeftThumbY = ScaleDown(report.LeftThumbY, true);
            report.RightThumbX = ScaleDown(report.RightThumbX, false);
            report.RightThumbY = ScaleDown(report.RightThumbY, true);
        }
        else if (report.AxisScaleInputType == InputAxisType.DualShock4 &&
                 InputSource.Configuration.OutputDeviceType != OutputDeviceType.DualShock4Controller)
        {
            report.LeftThumbX = ScaleUp(report.LeftThumbX, false);
            report.LeftThumbY = ScaleUp(report.LeftThumbY, true);
            report.RightThumbX = ScaleUp(report.RightThumbX, false);
            report.RightThumbY = ScaleUp(report.RightThumbY, true);
        }
    }

    private byte ScaleDown(short value, bool flip)
    {
        unchecked
        {
            var newValue = (byte)((value + 0x8000) / 257);
            if (flip)
            {
                newValue = (byte)(byte.MaxValue - newValue);
            }
            return newValue;
        }
    }

    private short ScaleUp(int Value, bool Flip)
    {
        unchecked
        {
            Value -= 0x80;
            float recipRun = Value >= 0 ? RecipInputPosResolution : RecipInputNegResolution;

            float temp = Value * recipRun;
            //if (Flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
            if (Flip)
            {
                temp = -temp;
            }

            temp = (temp + 1.0f) * 0.5f;

            return (short)((temp * OutputResolution) + -32768);
        }
    }
}