﻿using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

internal class Xbox360OutDevice : OutDevice
{
    //private const int inputResolution = 127 - (-128);
    //private const float reciprocalInputResolution = 1 / (float)inputResolution;
    private const float RecipInputPosResolution = 1 / 127f;
    private const float RecipInputNegResolution = 1 / 128f;
    private const int OutputResolution = 32767 - -32768;

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

    public override void ConvertAndSendReport(CompatibleHidDeviceInputReport state, int device = 0)
    {
        if (!IsConnected)
        {
            return;
        }

        cont.SetButtonState(Xbox360Button.Back, state.Share);
        cont.SetButtonState(Xbox360Button.LeftThumb, state.LeftThumb);
        cont.SetButtonState(Xbox360Button.RightThumb, state.RightThumb);
        cont.SetButtonState(Xbox360Button.Start, state.Options);
        cont.SetButtonState(Xbox360Button.Up, state.DPad == DPadDirection.North);
        cont.SetButtonState(Xbox360Button.Right, state.DPad == DPadDirection.East);
        cont.SetButtonState(Xbox360Button.Down, state.DPad == DPadDirection.South);
        cont.SetButtonState(Xbox360Button.Left, state.DPad == DPadDirection.West);
        cont.SetButtonState(Xbox360Button.LeftShoulder, state.LeftShoulder);
        cont.SetButtonState(Xbox360Button.RightShoulder, state.RightShoulder);
        cont.SetButtonState(Xbox360Button.Y, state.Triangle);
        cont.SetButtonState(Xbox360Button.B, state.Circle);
        cont.SetButtonState(Xbox360Button.A, state.Cross);
        cont.SetButtonState(Xbox360Button.X, state.Square);
        cont.SetButtonState(Xbox360Button.Guide, state.PS);
        cont.SetSliderValue(Xbox360Slider.LeftTrigger, state.LeftTrigger);
        cont.SetSliderValue(Xbox360Slider.RightTrigger, state.RightTrigger);

        cont.SetAxisValue(Xbox360Axis.LeftThumbX, AxisScale(state.LeftThumbX, false));
        cont.SetAxisValue(Xbox360Axis.LeftThumbY, AxisScale(state.LeftThumbY, true));
        cont.SetAxisValue(Xbox360Axis.RightThumbX, AxisScale(state.RightThumbX, false));
        cont.SetAxisValue(Xbox360Axis.RightThumbY, AxisScale(state.RightThumbY, true));

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

    public short AxisScale(int Value, bool Flip)
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