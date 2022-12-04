using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Output;

public class DS4OutDevice : OutDevice
{
    public IDualShock4Controller cont;

    //public DualShock4FeedbackReceivedEventHandler forceFeedbackCall;
    public Dictionary<int, DualShock4FeedbackReceivedEventHandler> forceFeedbacksDict = new();

    public DS4OutDevice(ViGEmClient client)
    {
        cont = client.CreateDualShock4Controller();
        //cont = client.CreateDualShock4Controller(0x054C, 0x09CC);
        cont.AutoSubmitReport = false;
    }

    public override void Connect()
    {
        cont.Connect();
        IsConnected = true;
    }

    public override void Disconnect()
    {
        RemoveFeedbacks();

        IsConnected = false;
        cont.Disconnect();

        cont = null;
    }

    public override OutputDeviceType GetDeviceType()
    {
        return OutputDeviceType.DualShock4Controller;
    }

    public override void RemoveFeedbacks()
    {
        foreach (KeyValuePair<int, DualShock4FeedbackReceivedEventHandler> pair in forceFeedbacksDict)
        {
            cont.FeedbackReceived -= pair.Value;
        }

        forceFeedbacksDict.Clear();
    }

    public override void RemoveFeedback(int inIdx)
    {
        if (forceFeedbacksDict.TryGetValue(inIdx, out DualShock4FeedbackReceivedEventHandler handler))
        {
            cont.FeedbackReceived -= handler;
            forceFeedbacksDict.Remove(inIdx);
        }
    }

    public override void ResetState(bool submit = true)
    {
        cont.ResetReport();
        if (submit)
        {
            cont.SubmitReport();
        }
    }

    public override void ConvertAndSendReport(CompatibleHidDeviceInputReport state, int device = 0)
    {
        if (!IsConnected)
        {
            return;
        }

        cont.SetButtonState(DualShock4Button.Share, state.Share);
        cont.SetButtonState(DualShock4Button.ThumbLeft, state.LeftThumb);
        cont.SetButtonState(DualShock4Button.ThumbRight, state.RightThumb);
        cont.SetButtonState(DualShock4Button.Options, state.Options);

        if (state.DPad == DPadDirection.NorthEast)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.Northeast);
        }
        else if (state.DPad == DPadDirection.NorthWest)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.Northwest);
        }
        else if (state.DPad == DPadDirection.North)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.North);
        }
        else if (state.DPad == DPadDirection.West)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.West);
        }
        else if (state.DPad == DPadDirection.East)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.East);
        }
        else if (state.DPad == DPadDirection.SouthEast)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.Southeast);
        }
        else if (state.DPad == DPadDirection.SouthWest)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.Southwest);
        }
        else if (state.DPad == DPadDirection.South)
        {
            cont.SetDPadDirection(DualShock4DPadDirection.South);
        }

        cont.SetButtonState(DualShock4Button.ShoulderLeft, state.LeftShoulder);
        cont.SetButtonState(DualShock4Button.ShoulderRight, state.RightShoulder);
        cont.SetButtonState(DualShock4Button.Triangle, state.Triangle);
        cont.SetButtonState(DualShock4Button.Circle, state.Circle);
        cont.SetButtonState(DualShock4Button.Cross, state.Cross);
        cont.SetButtonState(DualShock4Button.Square, state.Square);
        cont.SetButtonState(DualShock4SpecialButton.Ps, state.PS);
        cont.SetSliderValue(DualShock4Slider.LeftTrigger, state.LeftTrigger);
        cont.SetSliderValue(DualShock4Slider.RightTrigger, state.RightTrigger);

        cont.SetAxisValue(DualShock4Axis.LeftThumbX, state.LeftThumbX);
        cont.SetAxisValue(DualShock4Axis.LeftThumbY, state.LeftThumbY);
        cont.SetAxisValue(DualShock4Axis.RightThumbX, state.RightThumbX);
        cont.SetAxisValue(DualShock4Axis.RightThumbY, state.RightThumbY);

        cont.SubmitReport();
    }
}