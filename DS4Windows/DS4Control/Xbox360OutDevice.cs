using System.Collections.Generic;
using System.Linq;
using DS4Windows.VJoyFeeder;
using DS4WinWPF.DS4Control.IoC.Services;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DS4Windows
{
    public class Xbox360OutDevice : OutputDevice
    {
        //private const int inputResolution = 127 - (-128);
        //private const float reciprocalInputResolution = 1 / (float)inputResolution;
        private const float recipInputPosResolution = 1 / 127f;
        private const float recipInputNegResolution = 1 / 128f;
        private const int outputResolution = 32767 - -32768;
        public const string devType = "X360";

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

        public override void ConvertAndSendReport(DS4State state, int device)
        {
            if (!IsConnected) return;


            //cont.ResetReport();
            ushort tempButtons = 0;

            if (state.Share) tempButtons |= Xbox360Button.Back.Value;
            if (state.L3) tempButtons |= Xbox360Button.LeftThumb.Value;
            if (state.R3) tempButtons |= Xbox360Button.RightThumb.Value;
            if (state.Options) tempButtons |= Xbox360Button.Start.Value;

            if (state.DpadUp) tempButtons |= Xbox360Button.Up.Value;
            if (state.DpadRight) tempButtons |= Xbox360Button.Right.Value;
            if (state.DpadDown) tempButtons |= Xbox360Button.Down.Value;
            if (state.DpadLeft) tempButtons |= Xbox360Button.Left.Value;

            if (state.L1) tempButtons |= Xbox360Button.LeftShoulder.Value;
            if (state.R1) tempButtons |= Xbox360Button.RightShoulder.Value;

            if (state.Triangle) tempButtons |= Xbox360Button.Y.Value;
            if (state.Circle) tempButtons |= Xbox360Button.B.Value;
            if (state.Cross) tempButtons |= Xbox360Button.A.Value;
            if (state.Square) tempButtons |= Xbox360Button.X.Value;
            if (state.PS) tempButtons |= Xbox360Button.Guide.Value;
            cont.SetButtonsFull(tempButtons);

            cont.LeftTrigger = state.L2;
            cont.RightTrigger = state.R2;

            var steeringWheelMappedAxis =
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).SASteeringWheelEmulationAxis;
            switch (steeringWheelMappedAxis)
            {
                case SASteeringWheelEmulationAxisType.None:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.LX:
                    cont.LeftThumbX = (short)state.SASteeringWheelEmulationUnit;
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.LY:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = (short)state.SASteeringWheelEmulationUnit;
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.RX:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = (short)state.SASteeringWheelEmulationUnit;
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.RY:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = (short)state.SASteeringWheelEmulationUnit;
                    break;

                case SASteeringWheelEmulationAxisType.L2R2:
                    cont.LeftTrigger = cont.RightTrigger = 0;
                    if (state.SASteeringWheelEmulationUnit >= 0)
                        cont.LeftTrigger = (byte)state.SASteeringWheelEmulationUnit;
                    else cont.RightTrigger = (byte)state.SASteeringWheelEmulationUnit;
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1X:
                case SASteeringWheelEmulationAxisType.VJoy2X:
                    vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit,
                        ((uint)steeringWheelMappedAxis - (uint)SASteeringWheelEmulationAxisType.VJoy1X) / 3 + 1,
                        HID_USAGES.HID_USAGE_X);
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1Y:
                case SASteeringWheelEmulationAxisType.VJoy2Y:
                    vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit,
                        ((uint)steeringWheelMappedAxis - (uint)SASteeringWheelEmulationAxisType.VJoy1X) / 3 + 1,
                        HID_USAGES.HID_USAGE_Y);
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1Z:
                case SASteeringWheelEmulationAxisType.VJoy2Z:
                    vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit,
                        ((uint)steeringWheelMappedAxis - (uint)SASteeringWheelEmulationAxisType.VJoy1X) / 3 + 1,
                        HID_USAGES.HID_USAGE_Z);
                    goto case SASteeringWheelEmulationAxisType.None;

                default:
                    // Should never come here but just in case use the NONE case as default handler....
                    goto case SASteeringWheelEmulationAxisType.None;
            }

            cont.SubmitReport();
        }

        private short AxisScale(int Value, bool Flip)
        {
            unchecked
            {
                Value -= 0x80;
                var recipRun = Value >= 0 ? recipInputPosResolution : recipInputNegResolution;

                var temp = Value * recipRun;
                //if (Flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
                if (Flip) temp = -temp;
                temp = (temp + 1.0f) * 0.5f;

                return (short)(temp * outputResolution + -32768);
            }
        }

        public override void Connect()
        {
            cont.Connect();
            IsConnected = true;
        }

        public override void Disconnect()
        {
            foreach (var pair in forceFeedbacksDict) cont.FeedbackReceived -= pair.Value;

            forceFeedbacksDict.Clear();

            IsConnected = false;
            cont.Disconnect();
            cont = null;
        }

        public override string GetDeviceType()
        {
            return devType;
        }

        public override void ResetState(bool submit = true)
        {
            cont.ResetReport();
            if (submit) cont.SubmitReport();
        }

        public override void RemoveFeedbacks()
        {
            foreach (var pair in forceFeedbacksDict) cont.FeedbackReceived -= pair.Value;

            forceFeedbacksDict.Clear();
        }

        public override void RemoveFeedback(int inIdx)
        {
            if (forceFeedbacksDict.TryGetValue(inIdx, out var handler))
            {
                cont.FeedbackReceived -= handler;
                forceFeedbacksDict.Remove(inIdx);
            }
        }
    }
}