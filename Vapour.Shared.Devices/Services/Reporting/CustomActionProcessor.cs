using MessagePipe;

using Nefarius.Utilities.HID.Devices;

using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services.Reporting;

public class CustomActionProcessor : ICustomActionProcessor
{
    private readonly IAsyncPublisher<GracefulShutdownAction> _gracefulShutdownPublisher;
    private readonly IAsyncPublisher<SetPlayerLedAndColorAction> _setPlayerLedAndColorPublisher;
    public CustomActionReport CustomActionReport { get; } = new();
    private bool _wasLedExecuted;
    private bool _wasGracefulShutdownExecuted;

    public CustomActionProcessor(
        IAsyncPublisher<GracefulShutdownAction> gracefulShutdownPublisher,
        IAsyncPublisher<SetPlayerLedAndColorAction> setPlayerLedAndColorPublisher)
    {
        _gracefulShutdownPublisher = gracefulShutdownPublisher;
        _setPlayerLedAndColorPublisher = setPlayerLedAndColorPublisher;
    }
    
    public Task ProcessReport(IInputSource inputSource, InputSourceFinalReport inputReport)
    {
        long currentTicks = DateTime.Now.Ticks;
        CalculateBoolValue(currentTicks, inputReport.Cross, CustomActionReport.CrossStart);
        CalculateBoolValue(currentTicks, inputReport.Circle, CustomActionReport.CircleStart);
        CalculateBoolValue(currentTicks, inputReport.Triangle, CustomActionReport.TriangleStart);
        CalculateBoolValue(currentTicks, inputReport.Square, CustomActionReport.SquareStart);
        CalculateBoolValue(currentTicks, inputReport.DPad == DPadDirection.North, CustomActionReport.DPadNorthStart);
        CalculateBoolValue(currentTicks, inputReport.DPad == DPadDirection.East, CustomActionReport.DPadEastStart);
        CalculateBoolValue(currentTicks, inputReport.DPad == DPadDirection.South, CustomActionReport.DPadSouthStart);
        CalculateBoolValue(currentTicks, inputReport.DPad == DPadDirection.West, CustomActionReport.DPadWestStart);
        CalculateBoolValue(currentTicks, inputReport.Options, CustomActionReport.OptionsStart);
        CalculateBoolValue(currentTicks, inputReport.Share, CustomActionReport.ShareStart);
        CalculateBoolValue(currentTicks, inputReport.LeftShoulder, CustomActionReport.L1Start);
        CalculateByteValue(currentTicks, inputReport.LeftTrigger, CustomActionReport.L2Start);
        CalculateBoolValue(currentTicks, inputReport.LeftThumb, CustomActionReport.L3Start);
        CalculateBoolValue(currentTicks, inputReport.RightShoulder, CustomActionReport.R1Start);
        CalculateByteValue(currentTicks, inputReport.RightTrigger, CustomActionReport.R2Start);
        CalculateBoolValue(currentTicks, inputReport.RightThumb, CustomActionReport.R3Start);
        CalculateBoolValue(currentTicks, inputReport.PS, CustomActionReport.PSStart);

        CheckSetPlayerNumber(inputSource, inputReport, currentTicks);
        CheckPerformDisconnect(inputSource, currentTicks);

        return Task.CompletedTask;
    }

    private void CalculateBoolValue(long ticks, bool value, byte buttonIndex)
    {
        if (!value)
        {
            CustomActionReport.Values[buttonIndex] = 0;
        }
        else if (CustomActionReport.Values[buttonIndex] == 0)
        {
            CustomActionReport.Values[buttonIndex] = ticks;
        }
    }

    private void CalculateByteValue(long ticks, byte value, byte buttonIndex)
    {
        if (value != byte.MaxValue)
        {
            CustomActionReport.SetValue(buttonIndex, 0);
        }
        else if (CustomActionReport.GetValue(buttonIndex) == 0)
        {
            CustomActionReport.SetValue(buttonIndex, ticks);
        }
    }

    private void CheckSetPlayerNumber(IInputSource inputSource, InputSourceFinalReport inputReport, long currentTicks)
    {
        long r1Start = CustomActionReport.GetValue(CustomActionReport.R1Start);
        long r2Start = CustomActionReport.GetValue(CustomActionReport.R2Start);

        if (r1Start == 0 || r2Start == 0 || inputReport.DPad == DPadDirection.Default)
        {
            _wasLedExecuted = false;
        }

        if (r1Start > 0 && r2Start > 0)
        {
            byte playerNumber = inputReport.DPad switch
            {
                DPadDirection.North when currentTicks - r2Start > 1000000 && currentTicks - r1Start > 1000 => 1,
                DPadDirection.East when currentTicks - r2Start > 1000000 && currentTicks - r1Start > 1000 => 2,
                DPadDirection.South when currentTicks - r2Start > 1000000 && currentTicks - r1Start > 1000 => 3,
                DPadDirection.West when currentTicks - r2Start > 1000000 && currentTicks - r1Start > 1000 => 4,
                _ => 0
            };

            if (playerNumber > 0)
            {
                if (!_wasLedExecuted)
                {
                    _wasLedExecuted = true;

                    _setPlayerLedAndColorPublisher.Publish(new SetPlayerLedAndColorAction
                    {
                        InputSource = inputSource,
                        PlayerNumber = playerNumber
                    });
                }
            }
        }
    }

    private void CheckPerformDisconnect(IInputSource inputSource, long currentTicks)
    {
        long psStart = CustomActionReport.GetValue(CustomActionReport.PSStart);

        if (psStart == 0)
        {
            _wasGracefulShutdownExecuted = false;
        }
        else if (psStart > 0 && currentTicks - psStart > 40000000)
        {
            if (!_wasGracefulShutdownExecuted)
            {
                _wasGracefulShutdownExecuted = true;

                Task.Run(() =>
                {
                    _gracefulShutdownPublisher.Publish(new GracefulShutdownAction { InputSource = inputSource });
                });
            }
        }
    }
}
