using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;
public class CustomActionProcessor : ICustomActionProcessor
{
    public CustomActionReport CustomActionReport { get; } = new();

    public async Task ProcessReport(IInputSource inputSource, InputSourceFinalReport inputReport)
    {
        var currentTicks = DateTime.Now.Ticks;
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
        var r1Start = CustomActionReport.GetValue(CustomActionReport.R1Start);
        var r2Start = CustomActionReport.GetValue(CustomActionReport.R2Start);
        if (r1Start > 0 && r2Start > 0)
        {
            switch (inputReport.DPad)
            {
                case DPadDirection.North when currentTicks - r2Start > 1000 && currentTicks - r1Start > 1000:
                    inputSource.SetPlayerNumberAndColor(1);
                    break;
                case DPadDirection.East when currentTicks - r2Start > 1000 &&
                                             currentTicks - r1Start > 1000:
                    inputSource.SetPlayerNumberAndColor(2);
                    break;
                case DPadDirection.South when currentTicks - r2Start > 1000 &&
                                              currentTicks - r1Start > 1000:
                    inputSource.SetPlayerNumberAndColor(3);
                    break;
                case DPadDirection.West when currentTicks - r2Start > 1000 &&
                                             currentTicks - r1Start > 1000:
                    inputSource.SetPlayerNumberAndColor(4);
                    break;
            }
        }
    }

    private void CheckPerformDisconnect(IInputSource inputSource, long currentTicks)
    {
        var psStart = CustomActionReport.GetValue(CustomActionReport.PSStart);
        if (psStart > 0 && currentTicks - psStart > 40000000)
        {
            Task.Run(inputSource.DisconnectControllers);
        }
    }
}
