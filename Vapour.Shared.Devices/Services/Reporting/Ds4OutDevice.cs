using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

internal class DS4OutDevice : OutDevice
{
    private Thread _outDeviceThread;
    private CancellationTokenSource _outDeviceCancellationToken;
    private readonly IDualShock4Controller _controller;

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
            Priority = ThreadPriority.AboveNormal,
            IsBackground = true
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

        if (state.DPad == DPadDirection.NorthEast)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.Northeast);
        }
        else if (state.DPad == DPadDirection.NorthWest)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.Northwest);
        }
        else if (state.DPad == DPadDirection.North)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.North);
        }
        else if (state.DPad == DPadDirection.West)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.West);
        }
        else if (state.DPad == DPadDirection.East)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.East);
        }
        else if (state.DPad == DPadDirection.SouthEast)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.Southeast);
        }
        else if (state.DPad == DPadDirection.SouthWest)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.Southwest);
        }
        else if (state.DPad == DPadDirection.South)
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.South);
        }
        else
        {
            _controller.SetDPadDirection(DualShock4DPadDirection.None);
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
                bool timedOut;
                var buffer = _controller.AwaitRawOutputReport(100, out timedOut);

                if (!timedOut)
                {
                    FireOutputDeviceReportReceived(new OutputDeviceReport
                    {
                        OutputDeviceType = OutputDeviceType.DualShock4Controller, Packet = buffer.ToArray()
                    });
                }
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