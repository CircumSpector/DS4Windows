using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

internal class Xbox360OutDevice : OutDevice
{
    //private const int inputResolution = 127 - (-128);
    //private const float reciprocalInputResolution = 1 / (float)inputResolution;
    

    public IXbox360Controller cont;

    //public Xbox360FeedbackReceivedEventHandler forceFeedbackCall;
    // Input index, Xbox360FeedbackReceivedEventHandler instance
    public Dictionary<int, Xbox360FeedbackReceivedEventHandler> forceFeedbacksDict =
        new();

    public Xbox360OutDevice(ViGEmClient client)
    {
        cont = client.CreateXbox360Controller();
        cont.AutoSubmitReport = false;
    }

    public override void ConvertAndSendReport(InputSourceFinalReport state, int device = 0)
    {
        if (!IsConnected)
        {
            return;
        }

        cont.SetButtonState(Xbox360Button.Back, state.Share);
        cont.SetButtonState(Xbox360Button.LeftThumb, state.LeftThumb);
        cont.SetButtonState(Xbox360Button.RightThumb, state.RightThumb);
        cont.SetButtonState(Xbox360Button.Start, state.Options);
        SetDpad(state);
        cont.SetButtonState(Xbox360Button.LeftShoulder, state.LeftShoulder);
        cont.SetButtonState(Xbox360Button.RightShoulder, state.RightShoulder);
        cont.SetButtonState(Xbox360Button.Y, state.Triangle);
        cont.SetButtonState(Xbox360Button.B, state.Circle);
        cont.SetButtonState(Xbox360Button.A, state.Cross);
        cont.SetButtonState(Xbox360Button.X, state.Square);
        cont.SetButtonState(Xbox360Button.Guide, state.PS);
        cont.SetSliderValue(Xbox360Slider.LeftTrigger, state.LeftTrigger);
        cont.SetSliderValue(Xbox360Slider.RightTrigger, state.RightTrigger);

        cont.SetAxisValue(Xbox360Axis.LeftThumbX, state.LeftThumbX);
        cont.SetAxisValue(Xbox360Axis.LeftThumbY, state.LeftThumbY);
        cont.SetAxisValue(Xbox360Axis.RightThumbX, state.RightThumbX);
        cont.SetAxisValue(Xbox360Axis.RightThumbY, state.RightThumbY);

        cont.SubmitReport();
    }

    public override void Connect()
    {
        cont.Connect();
        IsConnected = true;
    }

    public override void Disconnect()
    {
        foreach (KeyValuePair<int, Xbox360FeedbackReceivedEventHandler> pair in forceFeedbacksDict)
        {
            cont.FeedbackReceived -= pair.Value;
        }

        forceFeedbacksDict.Clear();

        IsConnected = false;
        cont.Disconnect();
        cont = null;
    }

    public override OutputDeviceType GetDeviceType()
    {
        return OutputDeviceType.Xbox360Controller;
    }

    public override void ResetState(bool submit = true)
    {
        cont.ResetReport();
        if (submit)
        {
            cont.SubmitReport();
        }
    }

    public override void RemoveFeedbacks()
    {
        foreach (KeyValuePair<int, Xbox360FeedbackReceivedEventHandler> pair in forceFeedbacksDict)
        {
            cont.FeedbackReceived -= pair.Value;
        }

        forceFeedbacksDict.Clear();
    }

    public override void RemoveFeedback(int inIdx)
    {
        if (forceFeedbacksDict.TryGetValue(inIdx, out Xbox360FeedbackReceivedEventHandler handler))
        {
            cont.FeedbackReceived -= handler;
            forceFeedbacksDict.Remove(inIdx);
        }
    }

    private void SetDpad(InputSourceFinalReport state)
    {
        cont.SetButtonState(Xbox360Button.Up, state.DPad is DPadDirection.North or DPadDirection.NorthEast or DPadDirection.NorthWest);
        cont.SetButtonState(Xbox360Button.Right, state.DPad is DPadDirection.East or DPadDirection.NorthEast or DPadDirection.SouthEast);
        cont.SetButtonState(Xbox360Button.Down, state.DPad is DPadDirection.South or DPadDirection.SouthEast or DPadDirection.SouthWest);
        cont.SetButtonState(Xbox360Button.Left, state.DPad is DPadDirection.West or DPadDirection.SouthWest or DPadDirection.NorthWest);
    }
}