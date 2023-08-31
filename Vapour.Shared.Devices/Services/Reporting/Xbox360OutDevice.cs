using System.Threading.Channels;

using MessagePipe;

using Nefarius.Utilities.HID.Devices;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration.Messages;

namespace Vapour.Shared.Devices.Services.Reporting;

internal sealed class Xbox360OutDevice : OutDevice
{
    private readonly IAsyncPublisher<string, OutputDeviceReport> _outputDeviceReportPublisher;
    private readonly IXbox360Controller _controller;
    private CancellationTokenSource _outDeviceCancellationToken;
    private Channel<byte[]> _outDeviceReportChannel;
    private Thread _outDeviceThread;
    private IInputSource _inputSource;
    private string _outputDeviceReportReceivedKey;

    public Xbox360OutDevice(ViGEmClient client, IAsyncPublisher<string, OutputDeviceReport> outputDeviceReportPublisher)
    {
        _outputDeviceReportPublisher = outputDeviceReportPublisher;
        _controller = client.CreateXbox360Controller();
        _controller.AutoSubmitReport = false;
    }

    public override void ConvertAndSendReport(InputSourceFinalReport state, int device = 0)
    {
        if (!IsConnected)
        {
            return;
        }

        if (_inputSource == null)
        {
            _inputSource = state.InputSource;
            _outputDeviceReportReceivedKey =
                $"{_inputSource.InputSourceKey}_{MessageKeys.OutputDeviceReportReceivedKey}";
        }

        _controller.SetButtonState(Xbox360Button.Back, state.Share);
        _controller.SetButtonState(Xbox360Button.LeftThumb, state.LeftThumb);
        _controller.SetButtonState(Xbox360Button.RightThumb, state.RightThumb);
        _controller.SetButtonState(Xbox360Button.Start, state.Options);
        SetDpad(state);
        _controller.SetButtonState(Xbox360Button.LeftShoulder, state.LeftShoulder);
        _controller.SetButtonState(Xbox360Button.RightShoulder, state.RightShoulder);
        _controller.SetButtonState(Xbox360Button.Y, state.Triangle);
        _controller.SetButtonState(Xbox360Button.B, state.Circle);
        _controller.SetButtonState(Xbox360Button.A, state.Cross);
        _controller.SetButtonState(Xbox360Button.X, state.Square);
        _controller.SetButtonState(Xbox360Button.Guide, state.PS);
        _controller.SetSliderValue(Xbox360Slider.LeftTrigger, state.LeftTrigger);
        _controller.SetSliderValue(Xbox360Slider.RightTrigger, state.RightTrigger);

        _controller.SetAxisValue(Xbox360Axis.LeftThumbX, state.LeftThumbX);
        _controller.SetAxisValue(Xbox360Axis.LeftThumbY, state.LeftThumbY);
        _controller.SetAxisValue(Xbox360Axis.RightThumbX, state.RightThumbX);
        _controller.SetAxisValue(Xbox360Axis.RightThumbY, state.RightThumbY);

        _controller.SubmitReport();
    }

    public override void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        _outDeviceReportChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true
        });

        if (_outDeviceCancellationToken == null || _outDeviceCancellationToken.Token.IsCancellationRequested)
        {
            _outDeviceCancellationToken = new CancellationTokenSource();
        }

        _outDeviceThread = new Thread(ReceiveOutputDeviceReport)
        {
            Priority = ThreadPriority.AboveNormal, IsBackground = true
        };
        _outDeviceThread.Start();

        _controller.FeedbackReceived += ControllerFeedbackReceived;
        _controller.Connect();
        IsConnected = true;
    }

    public override void Disconnect()
    {
        if (!IsConnected)
        {
            return;
        }

        _controller.FeedbackReceived -= ControllerFeedbackReceived;
        _controller.Disconnect();

        _outDeviceCancellationToken.Cancel();
        _outDeviceThread.Join();
        _outDeviceReportChannel.Writer.Complete();
        _outDeviceCancellationToken.Dispose();
        _outDeviceCancellationToken = null;

        IsConnected = false;
    }

    public override OutputDeviceType GetDeviceType()
    {
        return OutputDeviceType.Xbox360Controller;
    }

    public override void ResetState(bool submit = true)
    {
        _controller.ResetReport();
        if (submit)
        {
            _controller.SubmitReport();
        }
    }

    private void SetDpad(InputSourceFinalReport state)
    {
        _controller.SetButtonState(Xbox360Button.Up,
            state.DPad is DPadDirection.North or DPadDirection.NorthEast or DPadDirection.NorthWest);
        _controller.SetButtonState(Xbox360Button.Right,
            state.DPad is DPadDirection.East or DPadDirection.NorthEast or DPadDirection.SouthEast);
        _controller.SetButtonState(Xbox360Button.Down,
            state.DPad is DPadDirection.South or DPadDirection.SouthEast or DPadDirection.SouthWest);
        _controller.SetButtonState(Xbox360Button.Left,
            state.DPad is DPadDirection.West or DPadDirection.SouthWest or DPadDirection.NorthWest);
    }

    private async void ReceiveOutputDeviceReport()
    {
        try
        {
            while (!_outDeviceCancellationToken.IsCancellationRequested)
            {
                byte[] buffer = await _outDeviceReportChannel.Reader.ReadAsync(_outDeviceCancellationToken.Token);
                if (_inputSource != null)
                {
                    await _outputDeviceReportPublisher.PublishAsync(_outputDeviceReportReceivedKey, new OutputDeviceReport
                    {
                        StrongMotor = buffer[0], WeakMotor = buffer[1],
                        InputSource = _inputSource,
                    });
                }
            }
        }
        catch
        {
        }
    }

    private async void ControllerFeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
    {
        await _outDeviceReportChannel.Writer.WriteAsync(new[] { e.LargeMotor, e.SmallMotor, e.LedNumber });
    }
}