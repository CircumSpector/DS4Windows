using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.SteamDeck.In;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public sealed class SteamDeckCompatibleInputReport : InputSourceReport, IStructInputSourceReport<InputReport>
{
    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;

    public void Parse(ref InputReport inputReport)
    {
        ReportId = inputReport.ReportId;
        var reportData = inputReport.InputReportData;

        var buttons0 = reportData.Buttons.Buttons0;
        Triangle = buttons0.HasFlag(SteamDeckButtons0.Y);
        Circle = buttons0.HasFlag(SteamDeckButtons0.B);
        Cross = buttons0.HasFlag(SteamDeckButtons0.A);
        Square = buttons0.HasFlag(SteamDeckButtons0.X);

        LeftShoulder = buttons0.HasFlag(SteamDeckButtons0.L1);
        RightShoulder = buttons0.HasFlag(SteamDeckButtons0.R1);

        Options = buttons0.HasFlag(SteamDeckButtons0.Options);
        Share = buttons0.HasFlag(SteamDeckButtons0.Menu);
        PS = buttons0.HasFlag(SteamDeckButtons0.Steam);

        SetDPad(buttons0);

        var buttons = reportData.Buttons;
        LeftThumb = buttons.LeftThumb;
        RightThumb = buttons.RightThumb;

        var sticksAndTriggers = reportData.SticksAndTriggers;
        LeftTrigger = sticksAndTriggers.LeftTrigger;
        RightTrigger = sticksAndTriggers.RightTrigger;

        LeftThumbX = sticksAndTriggers.LeftThumbX;
        LeftThumbY = sticksAndTriggers.LeftThumbY;
        RightThumbX = sticksAndTriggers.RightThumbX;
        RightThumbY = sticksAndTriggers.RightThumbY;
    }

    private void SetDPad(SteamDeckButtons0 buttons0)
    {
        if (buttons0.HasFlag(SteamDeckButtons0.DpadDown))
        {
            if (buttons0.HasFlag(SteamDeckButtons0.DpadLeft))
            {
                DPad = DPadDirection.SouthWest;
            }
            else if (buttons0.HasFlag(SteamDeckButtons0.DpadRight))
            {
                DPad = DPadDirection.SouthEast;
            }
            else
            {
                DPad = DPadDirection.South;
            }
        }
        else if (buttons0.HasFlag(SteamDeckButtons0.DpadLeft))
        {
            if (buttons0.HasFlag(SteamDeckButtons0.DpadUp))
            {
                DPad = DPadDirection.NorthWest;
            }
            else
            {
                DPad = DPadDirection.West;
            }
        }
        else if (buttons0.HasFlag(SteamDeckButtons0.DpadUp))
        {
            if (buttons0.HasFlag(SteamDeckButtons0.DpadRight))
            {
                DPad = DPadDirection.NorthEast;
            }
            else
            {
                DPad = DPadDirection.North;
            }
        }
        else if (buttons0.HasFlag(SteamDeckButtons0.DpadRight))
        {
            DPad = DPadDirection.East;
        }
        else
        {
            DPad = DPadDirection.Default;
        }
    }
}