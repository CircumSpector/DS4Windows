using System;
using System.Linq;
using DS4WinWPF.DS4Control.IoC.Services;
using OpenTracing.Util;

//using System.Diagnostics;

namespace DS4Windows
{
    public class Mouse : ITouchpadBehaviour
    {
        internal const int TRACKBALL_INIT_FICTION = 10;
        internal const int TRACKBALL_MASS = 45;
        internal const double TRACKBALL_RADIUS = 0.0245;
        private const int TRACKBALL_BUFFER_LEN = 8;

        private const int SMOOTH_BUFFER_LEN = 3;
        private readonly MouseWheel wheel;
        protected Mapping.Click clicked = Mapping.Click.None;

        private bool currentToggleGyroControls;
        private bool currentToggleGyroM;
        private bool currentToggleGyroStick;
        private readonly DS4Device dev;
        protected int deviceNum;

        public bool dragging, dragging2;

        private OneEuroFilterPair filterPair = new();
        protected Touch firstTouch, secondTouch;

        public GyroSwipeData gyroSwipe;

        // touch area stuff
        public bool leftDown, rightDown, upperDown, multiDown;
        protected DateTime pastTime, firstTap, TimeofEnd;
        private bool previousTriggerActivated;
        public bool priorLeftDown, priorRightDown, priorUpperDown, priorMultiDown;
        public bool priorSlideLeft, priorSlideright;
        public bool priorSwipeLeft, priorSwipeRight, priorSwipeUp, priorSwipeDown;
        public byte priorSwipeLeftB, priorSwipeRightB, priorSwipeUpB, priorSwipeDownB, priorSwipedB;
        protected DS4Controls pushed = DS4Controls.None;
        private DS4State s = new();
        public bool slideleft, slideright;
        private int smoothBufferTail;
        public bool swipeLeft, swipeRight, swipeUp, swipeDown;
        public byte swipeLeftB, swipeRightB, swipeUpB, swipeDownB, swipedB;
        private bool tappedOnce, secondtouchbegin;

        private bool tempBool;

        private bool toggleGyroControls = true;

        private bool toggleGyroMouse = true;

        private bool toggleGyroStick = true;

        private readonly double TRACKBALL_INERTIA = 2.0 * (TRACKBALL_MASS * TRACKBALL_RADIUS * TRACKBALL_RADIUS) / 5.0;
        private readonly double TRACKBALL_SCALE = 0.004;
        private double trackballAccel;
        private bool trackballActive;
        private int trackballBufferHead;
        private int trackballBufferTail;
        private double trackballDXRemain;
        private double trackballDYRemain;
        private readonly double[] trackballXBuffer = new double[TRACKBALL_BUFFER_LEN];
        private double trackballXVel;
        private readonly double[] trackballYBuffer = new double[TRACKBALL_BUFFER_LEN];
        private double trackballYVel;

        private bool triggeractivated;
        private bool useReverseRatchet;
        private readonly int[] xSmoothBuffer = new int[SMOOTH_BUFFER_LEN];
        private readonly int[] ySmoothBuffer = new int[SMOOTH_BUFFER_LEN];

        public Mouse(int deviceID, DS4Device d)
        {
            deviceNum = deviceID;
            dev = d;
            Cursor = new MouseCursor(deviceNum, d.GyroMouseSensSettings);
            wheel = new MouseWheel(deviceNum);
            trackballAccel = TRACKBALL_RADIUS * TRACKBALL_INIT_FICTION / TRACKBALL_INERTIA;
            firstTouch = new Touch(0, 0, 0);

            filterPair.Axis1Filter.MinCutoff = filterPair.Axis2Filter.MinCutoff = GyroMouseStickInfo.DefaultMinCutoff;
            filterPair.Axis1Filter.Beta = filterPair.Axis2Filter.Beta = GyroMouseStickInfo.DefaultBeta;
            ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo.SetRefreshEvents(filterPair.Axis1Filter);
            ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo.SetRefreshEvents(filterPair.Axis2Filter);
        }

        public int CursorGyroDead
        {
            get => Cursor.GyroCursorDeadZone;
            set => Cursor.GyroCursorDeadZone = value;
        }

        public bool ToggleGyroControls
        {
            get => toggleGyroControls;
            set
            {
                toggleGyroControls = value;
                ResetToggleGyroModes();
            }
        }

        public bool ToggleGyroMouse
        {
            get => toggleGyroMouse;
            set
            {
                toggleGyroMouse = value;
                ResetToggleGyroModes();
            }
        }

        public bool ToggleGyroStick
        {
            get => toggleGyroStick;
            set
            {
                toggleGyroStick = value;
                ResetToggleGyroModes();
            }
        }

        public MouseCursor Cursor { get; }

        public virtual void SixAxisMoved(DS4SixAxis sender, SixAxisEventArgs arg)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(SixAxisMoved)).StartActive(true);


            var outMode = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroOutputMode;
            if (outMode == GyroOutMode.Controls)
            {
                s = dev.GetCurrentStateReference();

                var controlsMapInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroControlsInfo;
                useReverseRatchet = controlsMapInfo.TriggerTurns;
                var i = 0;
                var ss = controlsMapInfo.Triggers.Split(',');
                var andCond = controlsMapInfo.TriggerCond;
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    var s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }

                        if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                if (toggleGyroControls)
                {
                    if (triggeractivated && triggeractivated != previousTriggerActivated)
                        currentToggleGyroStick = !currentToggleGyroStick;

                    previousTriggerActivated = triggeractivated;
                    triggeractivated = currentToggleGyroStick;
                }
                else
                {
                    previousTriggerActivated = triggeractivated;
                }

                if (useReverseRatchet && triggeractivated)
                    s.Motion.outputGyroControls = true;
                else if (!useReverseRatchet && !triggeractivated)
                    s.Motion.outputGyroControls = true;
                else
                    s.Motion.outputGyroControls = false;
            }
            else if (outMode == GyroOutMode.Mouse && ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroSensitivity > 0)
            {
                s = dev.GetCurrentStateReference();

                useReverseRatchet = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroTriggerTurns;
                var i = 0;
                var ss = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).SATriggers.Split(',');
                var andCond = Global.Instance.Config.GetSATriggerCondition(deviceNum);
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    var s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }

                        if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                if (toggleGyroMouse)
                {
                    if (triggeractivated && triggeractivated != previousTriggerActivated)
                        currentToggleGyroControls = !currentToggleGyroControls;

                    previousTriggerActivated = triggeractivated;
                    triggeractivated = currentToggleGyroControls;
                }
                else
                {
                    previousTriggerActivated = triggeractivated;
                }

                if (useReverseRatchet && triggeractivated)
                    Cursor.SixAxisMoved(arg);
                else if (!useReverseRatchet && !triggeractivated)
                    Cursor.SixAxisMoved(arg);
                else
                    Cursor.mouseRemainderReset(arg);
            }
            else if (outMode == GyroOutMode.MouseJoystick)
            {
                s = dev.GetCurrentStateReference();

                useReverseRatchet = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickTriggerTurns;
                var i = 0;
                var ss = Global.Instance.Config.GetSAMouseStickTriggers(deviceNum).Split(',');
                var andCond = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).SAMouseStickTriggerCond;
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    var s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }

                        if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                if (toggleGyroStick)
                {
                    if (triggeractivated && triggeractivated != previousTriggerActivated)
                        currentToggleGyroM = !currentToggleGyroM;

                    previousTriggerActivated = triggeractivated;
                    triggeractivated = currentToggleGyroM;
                }
                else
                {
                    previousTriggerActivated = triggeractivated;
                }

                if (useReverseRatchet && triggeractivated)
                    SixMouseStick(arg);
                else if (!useReverseRatchet && !triggeractivated)
                    SixMouseStick(arg);
                else
                    SixMouseReset(arg);
            }
            else if (outMode == GyroOutMode.DirectionalSwipe)
            {
                s = dev.GetCurrentStateReference();

                var swipeMapInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroSwipeInfo;
                useReverseRatchet = swipeMapInfo.TriggerTurns;
                var i = 0;
                var ss = swipeMapInfo.Triggers.Split(',');
                var andCond = swipeMapInfo.TriggerCondition;
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    var s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }

                        if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                gyroSwipe.previousSwipeLeft = gyroSwipe.swipeLeft;
                gyroSwipe.previousSwipeRight = gyroSwipe.swipeRight;
                gyroSwipe.previousSwipeUp = gyroSwipe.swipeUp;
                gyroSwipe.previousSwipeDown = gyroSwipe.swipeDown;

                if (useReverseRatchet && triggeractivated)
                    SixDirectionalSwipe(arg, swipeMapInfo);
                else if (!useReverseRatchet && !triggeractivated)
                    SixDirectionalSwipe(arg, swipeMapInfo);
                else
                    gyroSwipe.swipeLeft = gyroSwipe.swipeRight =
                        gyroSwipe.swipeUp = gyroSwipe.swipeDown = false;
            }
        }

        public virtual void TouchesMoved(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(TouchesMoved)).StartActive(true);


            s = dev.GetCurrentStateReference();

            var tempMode = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode;
            if (tempMode == TouchpadOutMode.Mouse)
            {
                if (Global.Instance.GetTouchActive(deviceNum))
                {
                    var disArray = Global.Instance.Config.TouchDisInvertTriggers[deviceNum];
                    tempBool = true;
                    for (int i = 0, arlen = disArray.Count; tempBool && i < arlen; i++)
                        if (getDS4ControlsByName(disArray[i]) == false)
                            tempBool = false;

                    if (ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TrackballMode)
                    {
                        var iIndex = trackballBufferTail;
                        // Establish 4 ms as the base
                        trackballXBuffer[iIndex] =
                            arg.touches[0].deltaX * TRACKBALL_SCALE / 0.004; // dev.getCurrentStateRef().elapsedTime;
                        trackballYBuffer[iIndex] =
                            arg.touches[0].deltaY * TRACKBALL_SCALE / 0.004; // dev.getCurrentStateRef().elapsedTime;
                        trackballBufferTail = (iIndex + 1) % TRACKBALL_BUFFER_LEN;
                        if (trackballBufferHead == trackballBufferTail)
                            trackballBufferHead = (trackballBufferHead + 1) % TRACKBALL_BUFFER_LEN;
                    }

                    Cursor.TouchesMoved(arg, dragging || dragging2, tempBool);
                    wheel.touchesMoved(arg, dragging || dragging2);
                }
                else
                {
                    if (ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TrackballMode)
                    {
                        var iIndex = trackballBufferTail;
                        trackballXBuffer[iIndex] = 0;
                        trackballYBuffer[iIndex] = 0;
                        trackballBufferTail = (iIndex + 1) % TRACKBALL_BUFFER_LEN;
                        if (trackballBufferHead == trackballBufferTail)
                            trackballBufferHead = (trackballBufferHead + 1) % TRACKBALL_BUFFER_LEN;
                    }
                }
            }
            else if (tempMode == TouchpadOutMode.Controls)
            {
                if (!(swipeUp || swipeDown || swipeLeft || swipeRight) && arg.touches.Length == 1)
                {
                    if (arg.touches[0].hwX - firstTouch.hwX > 300) swipeRight = true;
                    if (arg.touches[0].hwX - firstTouch.hwX < -300) swipeLeft = true;
                    if (arg.touches[0].hwY - firstTouch.hwY > 300) swipeDown = true;
                    if (arg.touches[0].hwY - firstTouch.hwY < -300) swipeUp = true;
                }

                swipeUpB = (byte)Math.Min(255, Math.Max(0, (firstTouch.hwY - arg.touches[0].hwY) * 1.5f));
                swipeDownB = (byte)Math.Min(255, Math.Max(0, (arg.touches[0].hwY - firstTouch.hwY) * 1.5f));
                swipeLeftB = (byte)Math.Min(255, Math.Max(0, firstTouch.hwX - arg.touches[0].hwX));
                swipeRightB = (byte)Math.Min(255, Math.Max(0, arg.touches[0].hwX - firstTouch.hwX));
            }
            else if (tempMode == TouchpadOutMode.AbsoluteMouse)
            {
                if (Global.Instance.GetTouchActive(deviceNum)) Cursor.TouchesMovedAbsolute(arg);
            }

            // Slide flags needed for possible profile switching from Touchpad swipes
            if (Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 50 && arg.touches.Length == 2)
            {
                if (arg.touches[0].hwX - firstTouch.hwX > 200 && !slideleft)
                    slideright = true;
                else if (firstTouch.hwX - arg.touches[0].hwX > 200 && !slideright)
                    slideleft = true;
            }

            SynthesizeMouseButtons();
        }

        public virtual void TouchesBegan(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(TouchesBegan)).StartActive(true);


            var tempMode = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode;
            var mouseMode = tempMode == TouchpadOutMode.Mouse;
            if (mouseMode)
            {
                Array.Clear(trackballXBuffer, 0, TRACKBALL_BUFFER_LEN);
                Array.Clear(trackballYBuffer, 0, TRACKBALL_BUFFER_LEN);
                trackballXVel = 0.0;
                trackballYVel = 0.0;
                trackballActive = false;
                trackballBufferTail = 0;
                trackballBufferHead = 0;
                trackballDXRemain = 0.0;
                trackballDYRemain = 0.0;

                Cursor.touchesBegan(arg);
                wheel.touchesBegan(arg);
            }

            pastTime = arg.timeStamp;
            firstTouch.Populate(arg.touches[0].hwX, arg.touches[0].hwY, arg.touches[0].touchID,
                arg.touches[0].previousTouch);

            if (mouseMode && ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).DoubleTap)
            {
                var test = arg.timeStamp;
                if (test <= firstTap +
                    TimeSpan.FromMilliseconds(ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TapSensitivity * 1.5) &&
                    !arg.touchButtonPressed)
                    secondtouchbegin = true;
            }

            s = dev.GetCurrentStateReference();
            SynthesizeMouseButtons();
        }

        public virtual void TouchesEnded(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(TouchesEnded)).StartActive(true);


            s = dev.GetCurrentStateReference();
            slideright = slideleft = false;
            swipeUp = swipeDown = swipeLeft = swipeRight = false;
            swipeUpB = swipeDownB = swipeLeftB = swipeRightB = 0;
            var tapSensitivity = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TapSensitivity;
            if (tapSensitivity != 0 && ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode == TouchpadOutMode.Mouse)
            {
                if (secondtouchbegin)
                {
                    tappedOnce = false;
                    secondtouchbegin = false;
                }

                var test = arg.timeStamp;
                if (test <= pastTime + TimeSpan.FromMilliseconds((double)tapSensitivity * 2) &&
                    !arg.touchButtonPressed && !tappedOnce)
                    if (Math.Abs(firstTouch.hwX - arg.touches[0].hwX) < 10 &&
                        Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 10)
                    {
                        if (ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).DoubleTap)
                        {
                            tappedOnce = true;
                            firstTap = arg.timeStamp;
                            TimeofEnd = DateTime.Now; //since arg can't be used in synthesizeMouseButtons
                        }
                        else
                        {
                            Mapping.MapClick(deviceNum, Mapping.Click.Left); //this way no delay if disabled
                        }
                    }
            }
            else
            {
                var tempMode = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode;
                if (tempMode == TouchpadOutMode.Mouse)
                {
                    var disArray = Global.Instance.Config.TouchDisInvertTriggers[deviceNum];
                    tempBool = true;
                    for (int i = 0, arlen = disArray.Count; tempBool && i < arlen; i++)
                        if (getDS4ControlsByName(disArray[i]) == false)
                            tempBool = false;

                    if (ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TrackballMode)
                    {
                        if (!trackballActive)
                        {
                            var currentWeight = 1.0;
                            var finalWeight = 0.0;
                            double x_out = 0.0, y_out = 0.0;
                            var idx = -1;
                            for (var i = 0; i < TRACKBALL_BUFFER_LEN && idx != trackballBufferHead; i++)
                            {
                                idx = (trackballBufferTail - i - 1 + TRACKBALL_BUFFER_LEN) % TRACKBALL_BUFFER_LEN;
                                x_out += trackballXBuffer[idx] * currentWeight;
                                y_out += trackballYBuffer[idx] * currentWeight;
                                finalWeight += currentWeight;
                                currentWeight *= 1.0;
                            }

                            x_out /= finalWeight;
                            trackballXVel = x_out;
                            y_out /= finalWeight;
                            trackballYVel = y_out;

                            trackballActive = true;
                        }

                        var tempAngle = Math.Atan2(-trackballYVel, trackballXVel);
                        var normX = Math.Abs(Math.Cos(tempAngle));
                        var normY = Math.Abs(Math.Sin(tempAngle));
                        var signX = Math.Sign(trackballXVel);
                        var signY = Math.Sign(trackballYVel);

                        var trackXvDecay = Math.Min(Math.Abs(trackballXVel), trackballAccel * s.elapsedTime * normX);
                        var trackYvDecay = Math.Min(Math.Abs(trackballYVel), trackballAccel * s.elapsedTime * normY);
                        var xVNew = trackballXVel - trackXvDecay * signX;
                        var yVNew = trackballYVel - trackYvDecay * signY;
                        var xMotion = xVNew * s.elapsedTime / TRACKBALL_SCALE;
                        var yMotion = yVNew * s.elapsedTime / TRACKBALL_SCALE;
                        if (xMotion != 0.0)
                            xMotion += trackballDXRemain;
                        else
                            trackballDXRemain = 0.0;

                        var dx = (int)xMotion;
                        trackballDXRemain = xMotion - dx;

                        if (yMotion != 0.0)
                            yMotion += trackballDYRemain;
                        else
                            trackballDYRemain = 0.0;

                        var dy = (int)yMotion;
                        trackballDYRemain = yMotion - dy;

                        trackballXVel = xVNew;
                        trackballYVel = yVNew;

                        if (dx == 0 && dy == 0)
                            trackballActive = false;
                        else
                            Cursor.TouchMoveCursor(dx, dy, tempBool);
                    }
                }
                else if (tempMode == TouchpadOutMode.AbsoluteMouse)
                {
                    var absMouseSettings = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchPadAbsMouse;
                    if (Global.Instance.GetTouchActive(deviceNum) && absMouseSettings.SnapToCenter)
                        Cursor.TouchCenterAbsolute();
                }
            }

            SynthesizeMouseButtons();
        }

        public virtual void TouchUnchanged(DS4Touchpad sender, EventArgs unused)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(TouchUnchanged)).StartActive(true);


            s = dev.GetCurrentStateReference();

            if (trackballActive)
                if (ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode == TouchpadOutMode.Mouse)
                {
                    var disArray = Global.Instance.Config.TouchDisInvertTriggers[deviceNum];
                    tempBool = true;
                    for (int i = 0, arlen = disArray.Count; tempBool && i < arlen; i++)
                        if (getDS4ControlsByName(disArray[i]) == false)
                            tempBool = false;

                    var tempAngle = Math.Atan2(-trackballYVel, trackballXVel);
                    var normX = Math.Abs(Math.Cos(tempAngle));
                    var normY = Math.Abs(Math.Sin(tempAngle));
                    var signX = Math.Sign(trackballXVel);
                    var signY = Math.Sign(trackballYVel);
                    var trackXvDecay = Math.Min(Math.Abs(trackballXVel), trackballAccel * s.elapsedTime * normX);
                    var trackYvDecay = Math.Min(Math.Abs(trackballYVel), trackballAccel * s.elapsedTime * normY);
                    var xVNew = trackballXVel - trackXvDecay * signX;
                    var yVNew = trackballYVel - trackYvDecay * signY;
                    var xMotion = xVNew * s.elapsedTime / TRACKBALL_SCALE;
                    var yMotion = yVNew * s.elapsedTime / TRACKBALL_SCALE;
                    if (xMotion != 0.0)
                        xMotion += trackballDXRemain;
                    else
                        trackballDXRemain = 0.0;

                    var dx = (int)xMotion;
                    trackballDXRemain = xMotion - dx;

                    if (yMotion != 0.0)
                        yMotion += trackballDYRemain;
                    else
                        trackballDYRemain = 0.0;

                    var dy = (int)yMotion;
                    trackballDYRemain = yMotion - dy;

                    trackballXVel = xVNew;
                    trackballYVel = yVNew;

                    if (dx == 0 && dy == 0)
                        trackballActive = false;
                    else
                        Cursor.TouchMoveCursor(dx, dy, tempBool);
                }

            if (s.Touch1Finger || s.TouchButton)
                SynthesizeMouseButtons();
        }

        public virtual void touchButtonUp(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            pushed = DS4Controls.None;
            upperDown = leftDown = rightDown = multiDown = false;
            s = dev.GetCurrentStateReference();
            if (s.Touch1 || s.Touch2)
                SynthesizeMouseButtons();
        }

        public virtual void touchButtonDown(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            if (arg.touches == null)
            {
                upperDown = true;
            }
            else if (arg.touches.Length > 1)
            {
                multiDown = true;
            }
            else
            {
                if (ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).LowerRCOn && arg.touches[0].hwX > 1920 * 3 / 4 &&
                    arg.touches[0].hwY > 960 * 3 / 4)
                    Mapping.MapClick(deviceNum, Mapping.Click.Right);

                if (isLeft(arg.touches[0]))
                    leftDown = true;
                else if (isRight(arg.touches[0]))
                    rightDown = true;
            }

            s = dev.GetCurrentStateReference();
            SynthesizeMouseButtons();
        }

        public void ResetTrackAccel(double friction)
        {
            trackballAccel = TRACKBALL_RADIUS * friction / TRACKBALL_INERTIA;
        }

        public void ResetToggleGyroModes()
        {
            currentToggleGyroControls = false;
            currentToggleGyroM = false;
            currentToggleGyroStick = false;

            previousTriggerActivated = false;
            triggeractivated = false;
        }

        public void ReplaceOneEuroFilterPair()
        {
            ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo.RemoveRefreshEvents();
            filterPair = new OneEuroFilterPair();
        }

        public void SetupLateOneEuroFilters()
        {
            filterPair.Axis1Filter.MinCutoff = filterPair.Axis2Filter.MinCutoff =
                ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo.MinCutoff;
            filterPair.Axis1Filter.Beta =
                filterPair.Axis2Filter.Beta = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo.Beta;
            ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo.SetRefreshEvents(filterPair.Axis1Filter);
            ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo.SetRefreshEvents(filterPair.Axis2Filter);
        }

        private void SixMouseReset(SixAxisEventArgs args)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(SixMouseReset)).StartActive(true);


            var iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
            xSmoothBuffer[iIndex] = 0;
            ySmoothBuffer[iIndex] = 0;
            smoothBufferTail = iIndex + 1;

            var msinfo = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo;
            if (msinfo.Smoothing == GyroMouseStickInfo.SmoothingMethod.OneEuro)
            {
                var currentRate = 1.0 / args.sixAxis.elapsed;
                filterPair.Axis1Filter.Filter(0.0, currentRate);
                filterPair.Axis2Filter.Filter(0.0, currentRate);
            }
        }

        private void SixMouseStick(SixAxisEventArgs arg)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(SixMouseStick)).StartActive(true);


            int deltaX = 0, deltaY = 0;
            deltaX = ProfilesService.Instance.ActiveProfiles.ElementAt(0).GyroMouseStickHorizontalAxis == 0
                ? arg.sixAxis.gyroYawFull
                : arg.sixAxis.gyroRollFull;
            deltaY = -arg.sixAxis.gyroPitchFull;
            //int inputX = deltaX, inputY = deltaY;
            var maxDirX = deltaX >= 0 ? 127 : -128;
            var maxDirY = deltaY >= 0 ? 127 : -128;

            var msinfo = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).GyroMouseStickInfo;

            var tempDouble = arg.sixAxis.elapsed * 250.0; // Base default speed on 4 ms
            var tempAngle = Math.Atan2(-deltaY, deltaX);
            var normX = Math.Abs(Math.Cos(tempAngle));
            var normY = Math.Abs(Math.Sin(tempAngle));
            var signX = Math.Sign(deltaX);
            var signY = Math.Sign(deltaY);

            var deadzoneX = (int)Math.Abs(normX * msinfo.DeadZone);
            var deadzoneY = (int)Math.Abs(normY * msinfo.DeadZone);

            var maxValX = signX * msinfo.MaxZone;
            var maxValY = signY * msinfo.MaxZone;

            double xratio = 0.0, yratio = 0.0;
            var antiX = msinfo.AntiDeadX * normX;
            var antiY = msinfo.AntiDeadY * normY;

            if (Math.Abs(deltaX) > deadzoneX)
            {
                deltaX -= signX * deadzoneX;
                //deltaX = (int)(deltaX * tempDouble);
                deltaX = deltaX < 0 && deltaX < maxValX ? maxValX :
                    deltaX > 0 && deltaX > maxValX ? maxValX : deltaX;
                //if (deltaX != maxValX) deltaX -= deltaX % (signX * GyroMouseFuzz);
            }
            else
            {
                deltaX = 0;
            }

            if (Math.Abs(deltaY) > deadzoneY)
            {
                deltaY -= signY * deadzoneY;
                //deltaY = (int)(deltaY * tempDouble);
                deltaY = deltaY < 0 && deltaY < maxValY ? maxValY :
                    deltaY > 0 && deltaY > maxValY ? maxValY : deltaY;
                //if (deltaY != maxValY) deltaY -= deltaY % (signY * GyroMouseFuzz);
            }
            else
            {
                deltaY = 0;
            }

            if (msinfo.UseSmoothing)
            {
                if (msinfo.Smoothing == GyroMouseStickInfo.SmoothingMethod.OneEuro)
                {
                    var currentRate = 1.0 / arg.sixAxis.elapsed;
                    deltaX = (int)filterPair.Axis1Filter.Filter(deltaX, currentRate);
                    deltaY = (int)filterPair.Axis2Filter.Filter(deltaY, currentRate);
                }
                else if (msinfo.Smoothing == GyroMouseStickInfo.SmoothingMethod.WeightedAverage)
                {
                    var iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
                    xSmoothBuffer[iIndex] = deltaX;
                    ySmoothBuffer[iIndex] = deltaY;
                    smoothBufferTail = iIndex + 1;

                    var currentWeight = 1.0;
                    var finalWeight = 0.0;
                    double x_out = 0.0, y_out = 0.0;
                    var idx = 0;
                    for (var i = 0; i < SMOOTH_BUFFER_LEN; i++)
                    {
                        idx = (smoothBufferTail - i - 1 + SMOOTH_BUFFER_LEN) % SMOOTH_BUFFER_LEN;
                        x_out += xSmoothBuffer[idx] * currentWeight;
                        y_out += ySmoothBuffer[idx] * currentWeight;
                        finalWeight += currentWeight;
                        currentWeight *= msinfo.SmoothWeight;
                    }

                    x_out /= finalWeight;
                    deltaX = (int)x_out;
                    y_out /= finalWeight;
                    deltaY = (int)y_out;
                }

                maxValX = deltaX < 0 ? -msinfo.MaxZone : msinfo.MaxZone;
                maxValY = deltaY < 0 ? -msinfo.MaxZone : msinfo.MaxZone;
                maxDirX = deltaX >= 0 ? 127 : -128;
                maxDirY = deltaY >= 0 ? 127 : -128;
            }

            if (msinfo.VerticalScale != 100)
            {
                var verticalScale = msinfo.VerticalScale * 0.01;
                deltaY = (int)(deltaY * verticalScale);
                deltaY = deltaY < 0 && deltaY < maxValY ? maxValY :
                    deltaY > 0 && deltaY > maxValY ? maxValY : deltaY;
            }

            if (deltaX != 0) xratio = deltaX / (double)maxValX;
            if (deltaY != 0) yratio = deltaY / (double)maxValY;

            if (msinfo.MaxOutputEnabled)
            {
                var maxOutRatio = msinfo.MaxOutput / 100.0;
                // Expand output a bit. Likely not going to get a straight line with Gyro
                var maxOutXRatio = Math.Min(normX / 0.99, 1.0) * maxOutRatio;
                var maxOutYRatio = Math.Min(normY / 0.99, 1.0) * maxOutRatio;

                xratio = Math.Min(Math.Max(xratio, 0.0), maxOutXRatio);
                yratio = Math.Min(Math.Max(yratio, 0.0), maxOutYRatio);
            }

            double xNorm = 0.0, yNorm = 0.0;
            if (xratio != 0.0) xNorm = (1.0 - antiX) * xratio + antiX;

            if (yratio != 0.0) yNorm = (1.0 - antiY) * yratio + antiY;

            if (msinfo.Inverted != 0)
            {
                if ((msinfo.Inverted & 1) == 1)
                    // Invert max dir value
                    maxDirX = deltaX >= 0 ? -128 : 127;

                if ((msinfo.Inverted & 2) == 2)
                    // Invert max dir value
                    maxDirY = deltaY >= 0 ? -128 : 127;
            }

            var axisXOut = (byte)(xNorm * maxDirX + 128.0);
            var axisYOut = (byte)(yNorm * maxDirY + 128.0);

            var outputX = msinfo.OutputHorizontal();
            var outputY = msinfo.OutputVertical();

            if (outputX) Mapping.gyroStickX[deviceNum] = axisXOut;

            if (outputY) Mapping.gyroStickY[deviceNum] = axisYOut;
        }

        private void SixDirectionalSwipe(SixAxisEventArgs arg, GyroDirectionalSwipeInfo swipeInfo)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(SixDirectionalSwipe)).StartActive(true);


            var velX = swipeInfo.XAxis == GyroDirectionalSwipeInfo.XAxisSwipe.Yaw
                ? arg.sixAxis.angVelYaw
                : arg.sixAxis.angVelRoll;
            var velY = arg.sixAxis.angVelPitch;
            var delayTime = swipeInfo.DelayTime;

            var deadzoneX = Math.Abs(swipeInfo.DeadZoneX);
            var deadzoneY = Math.Abs(swipeInfo.DeadZoneY);

            gyroSwipe.swipeLeft = gyroSwipe.swipeRight = false;
            if (Math.Abs(velX) > deadzoneX)
            {
                if (velX > 0)
                {
                    if (gyroSwipe.currentXDir != GyroSwipeData.XDir.Right)
                    {
                        gyroSwipe.initialTimeX = DateTime.Now;
                        gyroSwipe.currentXDir = GyroSwipeData.XDir.Right;
                        gyroSwipe.xActive = delayTime == 0;
                    }

                    if (gyroSwipe.xActive || (gyroSwipe.xActive =
                        gyroSwipe.initialTimeX + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                        gyroSwipe.swipeRight = true;
                }
                else
                {
                    if (gyroSwipe.currentXDir != GyroSwipeData.XDir.Left)
                    {
                        gyroSwipe.initialTimeX = DateTime.Now;
                        gyroSwipe.currentXDir = GyroSwipeData.XDir.Left;
                        gyroSwipe.xActive = delayTime == 0;
                    }

                    if (gyroSwipe.xActive || (gyroSwipe.xActive =
                        gyroSwipe.initialTimeX + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                        gyroSwipe.swipeLeft = true;
                }
            }
            else
            {
                gyroSwipe.currentXDir = GyroSwipeData.XDir.None;
            }

            gyroSwipe.swipeUp = gyroSwipe.swipeDown = false;
            if (Math.Abs(velY) > deadzoneY)
            {
                if (velY > 0)
                {
                    if (gyroSwipe.currentYDir != GyroSwipeData.YDir.Up)
                    {
                        gyroSwipe.initialTimeY = DateTime.Now;
                        gyroSwipe.currentYDir = GyroSwipeData.YDir.Up;
                        gyroSwipe.yActive = delayTime == 0;
                    }

                    if (gyroSwipe.yActive || (gyroSwipe.yActive =
                        gyroSwipe.initialTimeY + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                        gyroSwipe.swipeUp = true;
                }
                else
                {
                    if (gyroSwipe.currentYDir != GyroSwipeData.YDir.Down)
                    {
                        gyroSwipe.initialTimeY = DateTime.Now;
                        gyroSwipe.currentYDir = GyroSwipeData.YDir.Down;
                        gyroSwipe.yActive = delayTime == 0;
                    }

                    if (gyroSwipe.yActive || (gyroSwipe.yActive =
                        gyroSwipe.initialTimeY + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                        gyroSwipe.swipeDown = true;
                }
            }
            else
            {
                gyroSwipe.currentYDir = GyroSwipeData.YDir.None;
            }
        }

        private bool getDS4ControlsByName(int key)
        {
            switch (key)
            {
                case -1: return true;
                case 0: return s.Cross;
                case 1: return s.Circle;
                case 2: return s.Square;
                case 3: return s.Triangle;
                case 4: return s.L1;
                case 5: return s.L2 > 128;
                case 6: return s.R1;
                case 7: return s.R2 > 128;
                case 8: return s.DpadUp;
                case 9: return s.DpadDown;
                case 10: return s.DpadLeft;
                case 11: return s.DpadRight;
                case 12: return s.L3;
                case 13: return s.R3;
                case 14: return s.Touch1Finger;
                case 15: return s.Touch2Fingers;
                case 16: return s.Options;
                case 17: return s.Share;
                case 18: return s.PS;
                case 19: return s.TouchButton;
                case 20: return s.Mute;
            }

            return false;
        }

        private bool isLeft(Touch t)
        {
            return t.hwX < 1920 * 2 / 5;
        }

        private bool isRight(Touch t)
        {
            return t.hwX >= 1920 * 2 / 5;
        }

        private void SynthesizeMouseButtons()
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(SynthesizeMouseButtons)).StartActive(true);


            var tempMode = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode;
            if (tempMode != TouchpadOutMode.Passthru)
            {
                var touchClickPass = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchClickPassthru;
                if (!touchClickPass)
                    // Reset output Touchpad click button
                    s.OutputTouchButton = false;
            }
            else
            {
                // Don't allow virtual buttons for Passthru mode
                return;
            }

            if (Global.Instance.Config.GetDs4ControllerSetting(deviceNum, DS4Controls.TouchLeft).IsDefault &&
                leftDown)
            {
                Mapping.MapClick(deviceNum, Mapping.Click.Left);
                dragging2 = true;
            }
            else
            {
                dragging2 = false;
            }

            if (Global.Instance.Config.GetDs4ControllerSetting(deviceNum, DS4Controls.TouchUpper).IsDefault &&
                upperDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Middle);

            if (Global.Instance.Config.GetDs4ControllerSetting(deviceNum, DS4Controls.TouchRight).IsDefault &&
                rightDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Left);

            if (Global.Instance.Config.GetDs4ControllerSetting(deviceNum, DS4Controls.TouchMulti).IsDefault &&
                multiDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Right);

            if (ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode == TouchpadOutMode.Mouse)
            {
                if (tappedOnce)
                {
                    var tester = DateTime.Now;
                    if (tester > TimeofEnd +
                        TimeSpan.FromMilliseconds(ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TapSensitivity * 1.5))
                    {
                        Mapping.MapClick(deviceNum, Mapping.Click.Left);
                        tappedOnce = false;
                    }
                    //if it fails the method resets, and tries again with a new tester value (gives tap a delay so tap and hold can work)
                }

                if (secondtouchbegin) //if tap and hold (also works as double tap)
                {
                    Mapping.MapClick(deviceNum, Mapping.Click.Left);
                    dragging = true;
                }
                else
                {
                    dragging = false;
                }
            }
        }

        public void populatePriorButtonStates()
        {
            priorUpperDown = upperDown;
            priorLeftDown = leftDown;
            priorRightDown = rightDown;
            priorMultiDown = multiDown;

            priorSwipeLeft = swipeLeft;
            priorSwipeRight = swipeRight;
            priorSwipeUp = swipeUp;
            priorSwipeDown = swipeDown;
            priorSwipeLeftB = swipeLeftB;
            priorSwipeRightB = swipeRightB;
            priorSwipeUpB = swipeUpB;
            priorSwipeDownB = swipeDownB;
            priorSwipedB = swipedB;
        }

        public DS4State getDS4State()
        {
            return s;
        }

        public struct GyroSwipeData
        {
            public bool swipeLeft, swipeRight, swipeUp, swipeDown;
            public bool previousSwipeLeft, previousSwipeRight, previousSwipeUp, previousSwipeDown;

            public enum XDir : ushort
            {
                None,
                Left,
                Right
            }

            public enum YDir : ushort
            {
                None,
                Up,
                Down
            }

            public XDir currentXDir;
            public YDir currentYDir;
            public bool xActive;
            public bool yActive;

            public DateTime initialTimeX;
            public DateTime initialTimeY;
        }
    }
}