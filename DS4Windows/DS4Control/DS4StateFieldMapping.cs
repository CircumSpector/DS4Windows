using DS4Windows.Shared.Common.Types;

namespace DS4Windows
{
    public class DS4StateFieldMapping
    {
        public enum ControlType
        {
            Unknown = 0,
            Button,
            AxisDir,
            Trigger,
            Touch,
            GyroDir,
            SwipeDir
        }

        public static ControlType[] mappedType = new ControlType[50]
        {
            ControlType.Unknown, // DS4Controls.None
            ControlType.AxisDir, // DS4Controls.LXNeg
            ControlType.AxisDir, // DS4Controls.LXPos
            ControlType.AxisDir, // DS4Controls.LYNeg
            ControlType.AxisDir, // DS4Controls.LYPos
            ControlType.AxisDir, // DS4Controls.RXNeg
            ControlType.AxisDir, // DS4Controls.RXPos
            ControlType.AxisDir, // DS4Controls.RYNeg
            ControlType.AxisDir, // DS4Controls.RYPos
            ControlType.Button, // DS4Controls.L1
            ControlType.Trigger, // DS4Controls.L2
            ControlType.Button, // DS4Controls.L3
            ControlType.Button, // DS4Controls.R1
            ControlType.Trigger, // DS4Controls.R2
            ControlType.Button, // DS4Controls.R3
            ControlType.Button, // DS4Controls.Square
            ControlType.Button, // DS4Controls.Triangle
            ControlType.Button, // DS4Controls.Circle
            ControlType.Button, // DS4Controls.Cross
            ControlType.Button, // DS4Controls.DpadUp
            ControlType.Button, // DS4Controls.DpadRight
            ControlType.Button, // DS4Controls.DpadDown
            ControlType.Button, // DS4Controls.DpadLeft
            ControlType.Button, // DS4Controls.PS
            ControlType.Touch, // DS4Controls.TouchLeft
            ControlType.Touch, // DS4Controls.TouchUpper
            ControlType.Touch, // DS4Controls.TouchMulti
            ControlType.Touch, // DS4Controls.TouchRight
            ControlType.Button, // DS4Controls.Share
            ControlType.Button, // DS4Controls.Options
            ControlType.Button, // DS4Controls.Mute
            ControlType.GyroDir, // DS4Controls.GyroXPos
            ControlType.GyroDir, // DS4Controls.GyroXNeg
            ControlType.GyroDir, // DS4Controls.GyroZPos
            ControlType.GyroDir, // DS4Controls.GyroZNeg
            ControlType.SwipeDir, // DS4Controls.SwipeLeft
            ControlType.SwipeDir, // DS4Controls.SwipeRight
            ControlType.SwipeDir, // DS4Controls.SwipeUp
            ControlType.SwipeDir, // DS4Controls.SwipeDown
            ControlType.Button, // DS4Controls.L2FullPull
            ControlType.Button, // DS4Controls.R2FullPull
            ControlType.Button, // DS4Controls.GyroSwipeLeft
            ControlType.Button, // DS4Controls.GyroSwipeRight
            ControlType.Button, // DS4Controls.GyroSwipeUp
            ControlType.Button, // DS4Controls.GyroSwipeDown
            ControlType.Button, // DS4Controls.Capture
            ControlType.Button, // DS4Controls.SideL
            ControlType.Button, // DS4Controls.SideR
            ControlType.Trigger, // DS4Controls.LSOuter
            ControlType.Trigger // DS4Controls.RSOuter
        };

        public byte[] axisdirs = new byte[(int)DS4ControlItem.RSOuter + 1];

        public bool[] buttons = new bool[(int)DS4ControlItem.RSOuter + 1];
        public int[] gryodirs = new int[(int)DS4ControlItem.RSOuter + 1];
        public bool outputTouchButton;
        public bool[] swipedirbools = new bool[(int)DS4ControlItem.RSOuter + 1];
        public byte[] swipedirs = new byte[(int)DS4ControlItem.RSOuter + 1];
        public bool touchButton;
        public byte[] triggers = new byte[(int)DS4ControlItem.RSOuter + 1];

        public DS4StateFieldMapping()
        {
        }

        public DS4StateFieldMapping(DS4State cState, DS4StateExposed exposeState, Mouse tp, bool priorMouse = false)
        {
            PopulateFieldMapping(cState, exposeState, tp, priorMouse);
        }

        public void PopulateFieldMapping(DS4State cState, DS4StateExposed exposeState, Mouse tp,
            bool priorMouse = false)
        {
            unchecked
            {
                axisdirs[(int)DS4ControlItem.LXNeg] = cState.LX;
                axisdirs[(int)DS4ControlItem.LXPos] = cState.LX;
                axisdirs[(int)DS4ControlItem.LYNeg] = cState.LY;
                axisdirs[(int)DS4ControlItem.LYPos] = cState.LY;
                triggers[(int)DS4ControlItem.LSOuter] = cState.OutputLSOuter;

                axisdirs[(int)DS4ControlItem.RXNeg] = cState.RX;
                axisdirs[(int)DS4ControlItem.RXPos] = cState.RX;
                axisdirs[(int)DS4ControlItem.RYNeg] = cState.RY;
                axisdirs[(int)DS4ControlItem.RYPos] = cState.RY;
                triggers[(int)DS4ControlItem.RSOuter] = cState.OutputRSOuter;

                triggers[(int)DS4ControlItem.L2] = cState.L2;
                triggers[(int)DS4ControlItem.R2] = cState.R2;

                buttons[(int)DS4ControlItem.L1] = cState.L1;
                buttons[(int)DS4ControlItem.L2FullPull] = cState.L2 == 255;
                buttons[(int)DS4ControlItem.L3] = cState.L3;
                buttons[(int)DS4ControlItem.R1] = cState.R1;
                buttons[(int)DS4ControlItem.R2FullPull] = cState.R2 == 255;
                buttons[(int)DS4ControlItem.R3] = cState.R3;

                buttons[(int)DS4ControlItem.Cross] = cState.Cross;
                buttons[(int)DS4ControlItem.Triangle] = cState.Triangle;
                buttons[(int)DS4ControlItem.Circle] = cState.Circle;
                buttons[(int)DS4ControlItem.Square] = cState.Square;
                buttons[(int)DS4ControlItem.PS] = cState.PS;
                buttons[(int)DS4ControlItem.Options] = cState.Options;
                buttons[(int)DS4ControlItem.Share] = cState.Share;
                buttons[(int)DS4ControlItem.Mute] = cState.Mute;
                buttons[(int)DS4ControlItem.Capture] = cState.Capture;
                buttons[(int)DS4ControlItem.SideL] = cState.SideL;
                buttons[(int)DS4ControlItem.SideR] = cState.SideR;

                buttons[(int)DS4ControlItem.DpadUp] = cState.DpadUp;
                buttons[(int)DS4ControlItem.DpadRight] = cState.DpadRight;
                buttons[(int)DS4ControlItem.DpadDown] = cState.DpadDown;
                buttons[(int)DS4ControlItem.DpadLeft] = cState.DpadLeft;

                buttons[(int)DS4ControlItem.TouchLeft] = tp != null ? !priorMouse ? tp.leftDown : tp.priorLeftDown : false;
                buttons[(int)DS4ControlItem.TouchRight] =
                    tp != null ? !priorMouse ? tp.rightDown : tp.priorRightDown : false;
                buttons[(int)DS4ControlItem.TouchUpper] =
                    tp != null ? !priorMouse ? tp.upperDown : tp.priorUpperDown : false;
                buttons[(int)DS4ControlItem.TouchMulti] =
                    tp != null ? !priorMouse ? tp.multiDown : tp.priorMultiDown : false;

                var sixAxisX = -exposeState.getOutputAccelX();
                gryodirs[(int)DS4ControlItem.GyroXPos] = sixAxisX > 0 ? sixAxisX : 0;
                gryodirs[(int)DS4ControlItem.GyroXNeg] = sixAxisX < 0 ? sixAxisX : 0;

                var sixAxisZ = exposeState.getOutputAccelZ();
                gryodirs[(int)DS4ControlItem.GyroZPos] = sixAxisZ > 0 ? sixAxisZ : 0;
                gryodirs[(int)DS4ControlItem.GyroZNeg] = sixAxisZ < 0 ? sixAxisZ : 0;

                swipedirs[(int)DS4ControlItem.SwipeLeft] =
                    tp != null ? !priorMouse ? tp.swipeLeftB : tp.priorSwipeLeftB : (byte)0;
                swipedirs[(int)DS4ControlItem.SwipeRight] =
                    tp != null ? !priorMouse ? tp.swipeRightB : tp.priorSwipeRightB : (byte)0;
                swipedirs[(int)DS4ControlItem.SwipeUp] =
                    tp != null ? !priorMouse ? tp.swipeUpB : tp.priorSwipeUpB : (byte)0;
                swipedirs[(int)DS4ControlItem.SwipeDown] =
                    tp != null ? !priorMouse ? tp.swipeDownB : tp.priorSwipeDownB : (byte)0;

                swipedirbools[(int)DS4ControlItem.SwipeLeft] =
                    tp != null ? !priorMouse ? tp.swipeLeft : tp.priorSwipeLeft : false;
                swipedirbools[(int)DS4ControlItem.SwipeRight] =
                    tp != null ? !priorMouse ? tp.swipeRight : tp.priorSwipeRight : false;
                swipedirbools[(int)DS4ControlItem.SwipeUp] =
                    tp != null ? !priorMouse ? tp.swipeUp : tp.priorSwipeUp : false;
                swipedirbools[(int)DS4ControlItem.SwipeDown] =
                    tp != null ? !priorMouse ? tp.swipeDown : tp.priorSwipeDown : false;

                buttons[(int)DS4ControlItem.GyroSwipeLeft] = tp != null ? tp.gyroSwipe.swipeLeft : false;
                buttons[(int)DS4ControlItem.GyroSwipeRight] = tp != null ? tp.gyroSwipe.swipeRight : false;
                buttons[(int)DS4ControlItem.GyroSwipeUp] = tp != null ? tp.gyroSwipe.swipeUp : false;
                buttons[(int)DS4ControlItem.GyroSwipeDown] = tp != null ? tp.gyroSwipe.swipeDown : false;

                touchButton = cState.TouchButton;
                outputTouchButton = cState.OutputTouchButton;
            }
        }

        public void PopulateState(DS4State state)
        {
            unchecked
            {
                state.LX = axisdirs[(int)DS4ControlItem.LXNeg];
                state.LX = axisdirs[(int)DS4ControlItem.LXPos];
                state.LY = axisdirs[(int)DS4ControlItem.LYNeg];
                state.LY = axisdirs[(int)DS4ControlItem.LYPos];
                state.OutputLSOuter = triggers[(int)DS4ControlItem.LSOuter];

                state.RX = axisdirs[(int)DS4ControlItem.RXNeg];
                state.RX = axisdirs[(int)DS4ControlItem.RXPos];
                state.RY = axisdirs[(int)DS4ControlItem.RYNeg];
                state.RY = axisdirs[(int)DS4ControlItem.RYPos];
                state.OutputRSOuter = triggers[(int)DS4ControlItem.RSOuter];

                state.L2 = triggers[(int)DS4ControlItem.L2];
                state.R2 = triggers[(int)DS4ControlItem.R2];

                state.L1 = buttons[(int)DS4ControlItem.L1];
                state.L3 = buttons[(int)DS4ControlItem.L3];
                state.R1 = buttons[(int)DS4ControlItem.R1];
                state.R3 = buttons[(int)DS4ControlItem.R3];

                state.Cross = buttons[(int)DS4ControlItem.Cross];
                state.Triangle = buttons[(int)DS4ControlItem.Triangle];
                state.Circle = buttons[(int)DS4ControlItem.Circle];
                state.Square = buttons[(int)DS4ControlItem.Square];
                state.PS = buttons[(int)DS4ControlItem.PS];
                state.Options = buttons[(int)DS4ControlItem.Options];
                state.Share = buttons[(int)DS4ControlItem.Share];
                state.Mute = buttons[(int)DS4ControlItem.Mute];
                state.Capture = buttons[(int)DS4ControlItem.Capture];
                state.SideL = buttons[(int)DS4ControlItem.SideL];
                state.SideR = buttons[(int)DS4ControlItem.SideR];

                state.DpadUp = buttons[(int)DS4ControlItem.DpadUp];
                state.DpadRight = buttons[(int)DS4ControlItem.DpadRight];
                state.DpadDown = buttons[(int)DS4ControlItem.DpadDown];
                state.DpadLeft = buttons[(int)DS4ControlItem.DpadLeft];
                state.TouchButton = touchButton;
                state.OutputTouchButton = outputTouchButton;
            }
        }
    }
}