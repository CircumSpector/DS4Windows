using System;
using System.Linq;
using DS4WinWPF.DS4Control.IoC.Services;
using OpenTracing.Util;

namespace DS4Windows
{
    public class MouseCursor
    {
        /**
         * Indicate x/y direction for doing jitter compensation, etc.
         */
        public enum Direction
        {
            Negative,
            Neutral,
            Positive
        }

        public const int GYRO_MOUSE_DEADZONE = 10;
        private const double TOUCHPAD_MOUSE_OFFSET = 0.015;

        private const int SMOOTH_BUFFER_LEN = 3;
        private readonly int deviceNumber;


        private double coefficient;
        private OneEuroFilterPair filterPair = new();

        private readonly DS4Device.GyroMouseSens gyroMouseSensSettings;
        private bool gyroSmooth;

        private Direction hDirection = Direction.Neutral, vDirection = Direction.Neutral;

        // Track direction vector separately and very trivially for now.
        private Direction horizontalDirection = Direction.Neutral,
            verticalDirection = Direction.Neutral;

        // Keep track of remainders when performing moves or we lose fractional parts.
        private double horizontalRemainder, verticalRemainder;
        private double hRemainder, vRemainder;

        private byte lastTouchID;
        private int smoothBufferTail;
        private double tempDouble;

        private int tempInt;
        private double verticalScale;
        private readonly double[] xSmoothBuffer = new double[SMOOTH_BUFFER_LEN];
        private readonly double[] ySmoothBuffer = new double[SMOOTH_BUFFER_LEN];

        public MouseCursor(int deviceNum, DS4Device.GyroMouseSens gyroMouseSens)
        {
            deviceNumber = deviceNum;
            gyroMouseSensSettings = gyroMouseSens;
            filterPair.Axis1Filter.MinCutoff = filterPair.Axis2Filter.MinCutoff = GyroMouseInfo.DEFAULT_MINCUTOFF;
            filterPair.Axis1Filter.Beta = filterPair.Axis2Filter.Beta = GyroMouseInfo.DEFAULT_BETA;
            Global.Instance.Config.GyroMouseInfo[deviceNum].SetRefreshEvents(filterPair.Axis1Filter);
            Global.Instance.Config.GyroMouseInfo[deviceNum].SetRefreshEvents(filterPair.Axis2Filter);
        }

        public int GyroCursorDeadZone { get; set; } = GYRO_MOUSE_DEADZONE;

        public void ReplaceOneEuroFilterPair()
        {
            Global.Instance.Config.GyroMouseInfo[deviceNumber].RemoveRefreshEvents();
            filterPair = new OneEuroFilterPair();
        }

        public void SetupLateOneEuroFilters()
        {
            filterPair.Axis1Filter.MinCutoff = filterPair.Axis2Filter.MinCutoff =
                Global.Instance.Config.GyroMouseInfo[deviceNumber].MinCutoff;
            filterPair.Axis1Filter.Beta =
                filterPair.Axis2Filter.Beta = Global.Instance.Config.GyroMouseInfo[deviceNumber].Beta;
            Global.Instance.Config.GyroMouseInfo[deviceNumber].SetRefreshEvents(filterPair.Axis1Filter);
            Global.Instance.Config.GyroMouseInfo[deviceNumber].SetRefreshEvents(filterPair.Axis2Filter);
        }
        //bool tempBool = false;

        public virtual void SixAxisMoved(SixAxisEventArgs arg)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(SixAxisMoved)).StartActive(true);


            int deltaX = 0, deltaY = 0;
            deltaX = Global.Instance.Config.GetGyroMouseHorizontalAxis(deviceNumber) == 0
                ? arg.sixAxis.gyroYawFull
                : arg.sixAxis.gyroRollFull;
            deltaY = -arg.sixAxis.gyroPitchFull;
            //tempDouble = arg.sixAxis.elapsed * 0.001 * 200.0; // Base default speed on 5 ms
            tempDouble = arg.sixAxis.elapsed * 200.0; // Base default speed on 5 ms

            var tempInfo = Global.Instance.Config.GyroMouseInfo[deviceNumber];
            gyroSmooth = tempInfo.enableSmoothing;
            var gyroSmoothWeight = 0.0;

            coefficient = ProfilesService.Instance.ControllerSlotProfiles.ElementAt(deviceNumber).GyroSensitivity * 0.01 *
                          gyroMouseSensSettings.mouseCoefficient;
            var offset = gyroMouseSensSettings.mouseOffset;
            if (gyroSmooth) offset = gyroMouseSensSettings.mouseSmoothOffset;

            var tempAngle = Math.Atan2(-deltaY, deltaX);
            var normX = Math.Abs(Math.Cos(tempAngle));
            var normY = Math.Abs(Math.Sin(tempAngle));
            var signX = Math.Sign(deltaX);
            var signY = Math.Sign(deltaY);

            if (deltaX == 0 || hRemainder > 0 != deltaX > 0) hRemainder = 0.0;

            if (deltaY == 0 || vRemainder > 0 != deltaY > 0) vRemainder = 0.0;

            var deadzoneX = (int)Math.Abs(normX * GyroCursorDeadZone);
            var deadzoneY = (int)Math.Abs(normY * GyroCursorDeadZone);

            if (Math.Abs(deltaX) > deadzoneX)
                deltaX -= signX * deadzoneX;
            else
                deltaX = 0;

            if (Math.Abs(deltaY) > deadzoneY)
                deltaY -= signY * deadzoneY;
            else
                deltaY = 0;

            var xMotion = deltaX != 0
                ? coefficient * (deltaX * tempDouble)
                  + normX * (offset * signX)
                : 0;

            verticalScale = ProfilesService.Instance.ControllerSlotProfiles.ElementAt(deviceNumber).GyroSensVerticalScale * 0.01;
            var yMotion = deltaY != 0
                ? coefficient * verticalScale * (deltaY * tempDouble)
                  + normY * (offset * signY)
                : 0;

            var xAction = 0;
            if (xMotion != 0.0)
                xMotion += hRemainder;
            else
                hRemainder = 0.0;

            var yAction = 0;
            if (yMotion != 0.0)
                yMotion += vRemainder;
            else
                vRemainder = 0.0;

            if (gyroSmooth)
            {
                if (tempInfo.smoothingMethod == GyroMouseInfo.SmoothingMethod.OneEuro)
                {
                    var currentRate = 1.0 / arg.sixAxis.elapsed;
                    xMotion = filterPair.Axis1Filter.Filter(xMotion, currentRate);
                    yMotion = filterPair.Axis2Filter.Filter(yMotion, currentRate);
                }
                else
                {
                    var iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
                    xSmoothBuffer[iIndex] = xMotion;
                    ySmoothBuffer[iIndex] = yMotion;
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
                        currentWeight *= gyroSmoothWeight;
                    }

                    x_out /= finalWeight;
                    xMotion = x_out;
                    y_out /= finalWeight;
                    yMotion = y_out;
                }
            }

            hRemainder = vRemainder = 0.0;
            var distSqu = xMotion * xMotion + yMotion * yMotion;

            xAction = (int)xMotion;
            yAction = (int)yMotion;

            if (tempInfo.minThreshold == 1.0)
            {
                hRemainder = xMotion - xAction;
                vRemainder = yMotion - yAction;
            }
            else
            {
                if (distSqu >= tempInfo.minThreshold * tempInfo.minThreshold)
                {
                    hRemainder = xMotion - xAction;
                    vRemainder = yMotion - yAction;
                }
                else
                {
                    hRemainder = xMotion;
                    xAction = 0;

                    vRemainder = yMotion;
                    yAction = 0;
                }
            }

            var gyroInvert = ProfilesService.Instance.ControllerSlotProfiles.ElementAt(deviceNumber).GyroInvert;
            if ((gyroInvert & 0x02) == 2)
                xAction *= -1;

            if ((gyroInvert & 0x01) == 1)
                yAction *= -1;

            if (yAction != 0 || xAction != 0)
                Global.outputKBMHandler.MoveRelativeMouse(xAction, yAction);

            hDirection = xMotion > 0.0 ? Direction.Positive : xMotion < 0.0 ? Direction.Negative : Direction.Neutral;
            vDirection = yMotion > 0.0 ? Direction.Positive : yMotion < 0.0 ? Direction.Negative : Direction.Neutral;
        }

        public void mouseRemainderReset(SixAxisEventArgs arg)
        {
            hRemainder = vRemainder = 0.0;
            var iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
            xSmoothBuffer[iIndex] = 0.0;
            ySmoothBuffer[iIndex] = 0.0;
            smoothBufferTail = iIndex + 1;

            var tempInfo = Global.Instance.Config.GyroMouseInfo[deviceNumber];
            if (tempInfo.smoothingMethod == GyroMouseInfo.SmoothingMethod.OneEuro)
            {
                var currentRate = 1.0 / arg.sixAxis.elapsed;
                filterPair.Axis1Filter.Filter(0.0, currentRate);
                filterPair.Axis2Filter.Filter(0.0, currentRate);
            }
        }

        public void touchesBegan(TouchpadEventArgs arg)
        {
            if (arg.touches.Length == 1)
            {
                horizontalRemainder = verticalRemainder = 0.0;
                horizontalDirection = verticalDirection = Direction.Neutral;
            }
        }

        public void TouchesMoved(TouchpadEventArgs arg, bool dragging, bool disableInvert = false)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(TouchesMoved)).StartActive(true);


            var touchesLen = arg.touches.Length;
            if (!dragging && touchesLen != 1 || dragging && touchesLen < 1)
                return;

            int deltaX = 0, deltaY = 0;
            if (arg.touches[0].touchID != lastTouchID)
            {
                deltaX = deltaY = 0;
                horizontalRemainder = verticalRemainder = 0.0;
                horizontalDirection = verticalDirection = Direction.Neutral;
                lastTouchID = arg.touches[0].touchID;
            }
            else
            {
                if (dragging && touchesLen > 1)
                {
                    deltaX = arg.touches[1].deltaX;
                    deltaY = arg.touches[1].deltaY;
                }
                else
                {
                    deltaX = arg.touches[0].deltaX;
                    deltaY = arg.touches[0].deltaY;
                }
            }

            TouchMoveCursor(deltaX, deltaY, disableInvert);
        }

        public void TouchesMovedAbsolute(TouchpadEventArgs arg)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(TouchesMovedAbsolute)).StartActive(true);


            var touchesLen = arg.touches.Length;
            if (touchesLen != 1)
                return;

            int currentX = 0, currentY = 0;
            if (touchesLen > 1)
            {
                currentX = arg.touches[1].hwX;
                currentY = arg.touches[1].hwY;
            }
            else
            {
                currentX = arg.touches[0].hwX;
                currentY = arg.touches[0].hwY;
            }

            var absSettings = Global.Instance.Config.TouchPadAbsMouse[deviceNumber];

            var minX = (int)(DS4Touchpad.RES_HALFED_X - absSettings.MaxZoneX * 0.01 * DS4Touchpad.RES_HALFED_X);
            var minY = (int)(DS4Touchpad.RES_HALFED_Y - absSettings.MaxZoneY * 0.01 * DS4Touchpad.RES_HALFED_Y);
            var maxX = (int)(DS4Touchpad.RES_HALFED_X + absSettings.MaxZoneX * 0.01 * DS4Touchpad.RES_HALFED_X);
            var maxY = (int)(DS4Touchpad.RES_HALFED_Y + absSettings.MaxZoneY * 0.01 * DS4Touchpad.RES_HALFED_Y);

            var mX = (DS4Touchpad.RESOLUTION_X_MAX - 0) / (double)(maxX - minX);
            var bX = minX * mX;
            var mY = (DS4Touchpad.RESOLUTION_Y_MAX - 0) / (double)(maxY - minY);
            var bY = minY * mY;

            currentX = currentX > maxX ? maxX : currentX < minX ? minX : currentX;
            currentY = currentY > maxY ? maxY : currentX < minY ? minY : currentY;

            var absX = (currentX * mX - bX) / DS4Touchpad.RESOLUTION_X_MAX;
            var absY = (currentY * mY - bY) / DS4Touchpad.RESOLUTION_Y_MAX;
            //InputMethods.MoveAbsoluteMouse(absX, absY);
            Global.outputKBMHandler.MoveAbsoluteMouse(absX, absY);
        }

        public void TouchCenterAbsolute()
        {
            //InputMethods.MoveAbsoluteMouse(0.5, 0.5);
            Global.outputKBMHandler.MoveAbsoluteMouse(0.5, 0.5);
        }

        public void TouchMoveCursor(int dx, int dy, bool disableInvert = false)
        {
            using var scope = GlobalTracer.Instance.BuildSpan(nameof(TouchMoveCursor)).StartActive(true);


            var relMouseSettings = Global.Instance.Config.TouchPadRelMouse[deviceNumber];
            if (relMouseSettings.Rotation != 0.0)
            {
                //double rotation = 5.0 * Math.PI / 180.0;
                var rotation = relMouseSettings.Rotation;
                double sinAngle = Math.Sin(rotation), cosAngle = Math.Cos(rotation);
                int tempX = dx, tempY = dy;
                dx = (int)Global.Clamp(-DS4Touchpad.RESOLUTION_X_MAX, tempX * cosAngle - tempY * sinAngle,
                    DS4Touchpad.RESOLUTION_X_MAX);
                dy = (int)Global.Clamp(-DS4Touchpad.RESOLUTION_Y_MAX, tempX * sinAngle + tempY * cosAngle,
                    DS4Touchpad.RESOLUTION_Y_MAX);
            }

            var tempAngle = Math.Atan2(-dy, dx);
            var normX = Math.Abs(Math.Cos(tempAngle));
            var normY = Math.Abs(Math.Sin(tempAngle));
            var signX = Math.Sign(dx);
            var signY = Math.Sign(dy);
            var coefficient = ProfilesService.Instance.ControllerSlotProfiles.ElementAt(deviceNumber).TouchSensitivity * 0.01;
            var jitterCompenstation = ProfilesService.Instance.ControllerSlotProfiles.ElementAt(deviceNumber).TouchpadJitterCompensation;

            var xMotion = dx != 0 ? coefficient * dx + normX * (TOUCHPAD_MOUSE_OFFSET * signX) : 0.0;

            var yMotion = dy != 0 ? coefficient * dy + normY * (TOUCHPAD_MOUSE_OFFSET * signY) : 0.0;

            if (jitterCompenstation)
            {
                var absX = Math.Abs(xMotion);
                if (absX <= normX * 0.15) xMotion = signX * Math.Pow(absX / 0.15f, 1.408) * 0.15;

                var absY = Math.Abs(yMotion);
                if (absY <= normY * 0.15) yMotion = signY * Math.Pow(absY / 0.15f, 1.408) * 0.15;
            }

            // Collect rounding errors instead of losing motion.
            if (xMotion > 0.0 && horizontalRemainder > 0.0)
                xMotion += horizontalRemainder;
            else if (xMotion < 0.0 && horizontalRemainder < 0.0) xMotion += horizontalRemainder;

            if (yMotion > 0.0 && verticalRemainder > 0.0)
                yMotion += verticalRemainder;
            else if (yMotion < 0.0 && verticalRemainder < 0.0) yMotion += verticalRemainder;

            var distSqu = xMotion * xMotion + yMotion * yMotion;
            var xAction = (int)xMotion;
            var yAction = (int)yMotion;

            if (relMouseSettings.MinThreshold == 1.0)
            {
                horizontalRemainder = xMotion - xAction;
                verticalRemainder = yMotion - yAction;
            }
            else
            {
                //Console.WriteLine("{0} {1}", horizontalRemainder, xAction, distSqu);

                if (distSqu >= relMouseSettings.MinThreshold * relMouseSettings.MinThreshold)
                {
                    horizontalRemainder = xMotion - xAction;
                    verticalRemainder = yMotion - yAction;
                }
                else
                {
                    horizontalRemainder = xMotion;
                    xAction = 0;

                    verticalRemainder = yMotion;
                    yAction = 0;
                }
            }

            if (disableInvert == false)
            {
                var touchpadInvert = tempInt = Global.Instance.Config.GetTouchPadInvert(deviceNumber);
                if ((touchpadInvert & 0x02) == 2)
                    xAction *= -1;

                if ((touchpadInvert & 0x01) == 1)
                    yAction *= -1;
            }

            if (yAction != 0 || xAction != 0)
                Global.outputKBMHandler.MoveRelativeMouse(xAction, yAction);

            horizontalDirection = xMotion > 0.0 ? Direction.Positive :
                xMotion < 0.0 ? Direction.Negative : Direction.Neutral;
            verticalDirection = yMotion > 0.0 ? Direction.Positive :
                yMotion < 0.0 ? Direction.Negative : Direction.Neutral;
        }
    }
}