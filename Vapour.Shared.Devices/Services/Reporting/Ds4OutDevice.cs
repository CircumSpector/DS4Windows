using System.Diagnostics.CodeAnalysis;

using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class DS4OutDevice : OutDevice
{
    private readonly IDualShock4Controller _controller;
    private CancellationTokenSource _outDeviceCancellationToken;
    private Thread _outDeviceThread;

    public DS4OutDevice(ViGEmClient client)
    {
        _controller = client.CreateDualShock4Controller();
        _controller.AutoSubmitReport = false;
    }

    public override void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        if (_outDeviceCancellationToken == null || _outDeviceCancellationToken.Token.IsCancellationRequested)
        {
            _outDeviceCancellationToken = new CancellationTokenSource();
        }

        _outDeviceThread = new Thread(ReceiveOutputDeviceReport)
        {
            Priority = ThreadPriority.AboveNormal, IsBackground = true
        };
        _outDeviceThread.Start();

        _controller.Connect();
        IsConnected = true;
    }

    public override void Disconnect()
    {
        if (!IsConnected)
        {
            return;
        }

        _controller.Disconnect();
        _controller.Dispose();

        _outDeviceCancellationToken.Cancel();
        _outDeviceThread.Join();
        _outDeviceCancellationToken.Dispose();
        _outDeviceCancellationToken = null;

        IsConnected = false;
    }

    public override OutputDeviceType GetDeviceType()
    {
        return OutputDeviceType.DualShock4Controller;
    }

    public override void ResetState(bool submit = true)
    {
        _controller.ResetReport();
        if (submit)
        {
            _controller.SubmitReport();
        }
    }

    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    public override void ConvertAndSendReport(InputSourceFinalReport state, int device = 0)
    {
        if (!IsConnected)
        {
            return;
        }

        _controller.SetButtonState(DualShock4Button.Share, state.Share);
        _controller.SetButtonState(DualShock4Button.ThumbLeft, state.LeftThumb);
        _controller.SetButtonState(DualShock4Button.ThumbRight, state.RightThumb);
        _controller.SetButtonState(DualShock4Button.Options, state.Options);

        switch (state.DPad)
        {
            case DPadDirection.NorthEast:
                _controller.SetDPadDirection(DualShock4DPadDirection.Northeast);
                break;
            case DPadDirection.NorthWest:
                _controller.SetDPadDirection(DualShock4DPadDirection.Northwest);
                break;
            case DPadDirection.North:
                _controller.SetDPadDirection(DualShock4DPadDirection.North);
                break;
            case DPadDirection.West:
                _controller.SetDPadDirection(DualShock4DPadDirection.West);
                break;
            case DPadDirection.East:
                _controller.SetDPadDirection(DualShock4DPadDirection.East);
                break;
            case DPadDirection.SouthEast:
                _controller.SetDPadDirection(DualShock4DPadDirection.Southeast);
                break;
            case DPadDirection.SouthWest:
                _controller.SetDPadDirection(DualShock4DPadDirection.Southwest);
                break;
            case DPadDirection.South:
                _controller.SetDPadDirection(DualShock4DPadDirection.South);
                break;
            default:
                _controller.SetDPadDirection(DualShock4DPadDirection.None);
                break;
        }

        _controller.SetButtonState(DualShock4Button.ShoulderLeft, state.LeftShoulder);
        _controller.SetButtonState(DualShock4Button.ShoulderRight, state.RightShoulder);
        _controller.SetButtonState(DualShock4Button.Triangle, state.Triangle);
        _controller.SetButtonState(DualShock4Button.Circle, state.Circle);
        _controller.SetButtonState(DualShock4Button.Cross, state.Cross);
        _controller.SetButtonState(DualShock4Button.Square, state.Square);
        _controller.SetButtonState(DualShock4SpecialButton.Ps, state.PS);
        _controller.SetSliderValue(DualShock4Slider.LeftTrigger, state.LeftTrigger);
        _controller.SetSliderValue(DualShock4Slider.RightTrigger, state.RightTrigger);

        _controller.SetAxisValue(DualShock4Axis.LeftThumbX, (byte)state.LeftThumbX);
        _controller.SetAxisValue(DualShock4Axis.LeftThumbY, (byte)state.LeftThumbY);
        _controller.SetAxisValue(DualShock4Axis.RightThumbX, (byte)state.RightThumbX);
        _controller.SetAxisValue(DualShock4Axis.RightThumbY, (byte)state.RightThumbY);

        _controller.SubmitReport();
    }

    private void ReceiveOutputDeviceReport()
    {
        try
        {
            while (!_outDeviceCancellationToken.IsCancellationRequested)
            {
                IEnumerable<byte> buffer = _controller.AwaitRawOutputReport(250, out bool timedOut);

                if (timedOut)
                {
                    continue;
                }

                byte[] content = buffer.ToArray();

                FireOutputDeviceReportReceived(new OutputDeviceReport
                {
                    StrongMotor = content[5], WeakMotor = content[4]
                });
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
        }
    }
}