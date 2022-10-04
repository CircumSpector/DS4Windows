﻿using System.Diagnostics;

using Vapour.Shared.Common.Legacy;

namespace Vapour.Shared.Common.Types;

public delegate void SixAxisHandler<TEventArgs>(DS4SixAxis sender, TEventArgs args);

public class SixAxisEventArgs : EventArgs
{
    public readonly SixAxis sixAxis;
    public readonly DateTime timeStamp;

    public SixAxisEventArgs(DateTime utcTimestamp, SixAxis sa)
    {
        sixAxis = sa;
        timeStamp = utcTimestamp;
    }
}

public class SixAxis
{
    public const int ACC_RES_PER_G = 8192;
    public const float F_ACC_RES_PER_G = ACC_RES_PER_G;
    public const int GYRO_RES_IN_DEG_SEC = 16;
    public const float F_GYRO_RES_IN_DEG_SEC = GYRO_RES_IN_DEG_SEC;
    public int accelXFull, accelYFull, accelZFull;
    public double accelXG, accelYG, accelZG;
    public double angVelYaw, angVelPitch, angVelRoll;
    public double elapsed;

    public int gyroYaw, gyroPitch, gyroRoll, accelX, accelY, accelZ;
    public int gyroYawFull, gyroPitchFull, gyroRollFull;
    public int outputAccelX, outputAccelY, outputAccelZ;
    public bool outputGyroControls;
    public SixAxis previousAxis;

    private double tempDouble;

    public SixAxis(int X, int Y, int Z,
        int aX, int aY, int aZ,
        double elapsedDelta, SixAxis prevAxis = null)
    {
        Populate(X, Y, Z, aX, aY, aZ, elapsedDelta, prevAxis);
    }

    public void CopyFrom(SixAxis src)
    {
        gyroYaw = src.gyroYaw;
        gyroPitch = src.gyroPitch;
        gyroRoll = src.gyroRoll;

        gyroYawFull = src.gyroYawFull;
        accelXFull = src.accelXFull;
        accelYFull = src.accelYFull;
        accelZFull = src.accelZFull;

        angVelYaw = src.angVelYaw;
        angVelPitch = src.angVelPitch;
        angVelRoll = src.angVelRoll;

        accelXG = src.accelXG;
        accelYG = src.accelYG;
        accelZG = src.accelZG;

        // Put accel ranges between 0 - 128 abs
        accelX = src.accelX;
        accelY = src.accelY;
        accelZ = src.accelZ;
        outputAccelX = accelX;
        outputAccelY = accelY;
        outputAccelZ = accelZ;

        elapsed = src.elapsed;
        previousAxis = src.previousAxis;
        outputGyroControls = src.outputGyroControls;
    }

    public void Populate(int X, int Y, int Z,
        int aX, int aY, int aZ,
        double elapsedDelta, SixAxis prevAxis = null)
    {
        gyroYaw = -X / 256;
        gyroPitch = Y / 256;
        gyroRoll = -Z / 256;

        gyroYawFull = -X;
        gyroPitchFull = Y;
        gyroRollFull = -Z;
        accelXFull = -aX;
        accelYFull = -aY;
        accelZFull = aZ;

        angVelYaw = gyroYawFull / F_GYRO_RES_IN_DEG_SEC;
        angVelPitch = gyroPitchFull / F_GYRO_RES_IN_DEG_SEC;
        angVelRoll = gyroRollFull / F_GYRO_RES_IN_DEG_SEC;

        accelXG = tempDouble = accelXFull / F_ACC_RES_PER_G;
        accelYG = tempDouble = accelYFull / F_ACC_RES_PER_G;
        accelZG = tempDouble = accelZFull / F_ACC_RES_PER_G;

        // Put accel ranges between 0 - 128 abs
        accelX = -aX / 64;
        accelY = -aY / 64;
        accelZ = aZ / 64;

        // Leave blank and have mapping routine alter values as needed
        outputAccelX = 0;
        outputAccelY = 0;
        outputAccelZ = 0;
        outputGyroControls = false;

        elapsed = elapsedDelta;
        previousAxis = prevAxis;
    }
}

internal class CalibData
{
    public const int GyroPitchIdx = 0,
        GyroYawIdx = 1,
        GyroRollIdx = 2,
        AccelXIdx = 3,
        AccelYIdx = 4,
        AccelZIdx = 5;

    public int bias;
    public int sensDenom;
    public int sensNumer;
}

public class GyroAverageWindow
{
    public double accelMagnitude;
    public int numSamples;
    public DateTime start;
    public DateTime stop;
    public int x;
    public int y;
    public int z;

    public GyroAverageWindow()
    {
        Reset();
    }

    public int DurationMs // property
    {
        get
        {
            var timeDiff = stop - start;
            return Convert.ToInt32(timeDiff.TotalMilliseconds);
        }
    }

    public void Reset()
    {
        x = y = z = numSamples = 0;
        accelMagnitude = 0.0;
        start = stop = DateTime.UtcNow;
    }

    public bool StopIfElapsed(int ms)
    {
        var end = DateTime.UtcNow;
        var timeDiff = end - start;
        var shouldStop = Convert.ToInt32(timeDiff.TotalMilliseconds) >= ms;
        if (!shouldStop) stop = end;
        return shouldStop;
    }

    public double GetWeight(int expectedMs)
    {
        if (expectedMs == 0) return 0;
        return Math.Min(1.0, DurationMs / expectedMs);
    }
}

public class DS4SixAxis
{
    // for continuous calibration (JoyShockLibrary)
    private const int num_gyro_average_windows = 3;
    private const int gyro_average_window_ms = 5000;

    private readonly CalibData[] calibrationData = new CalibData[6]
    {
        new(), new(),
        new(), new(), new(), new()
    };

    private readonly GyroAverageWindow[] gyro_average_window = new GyroAverageWindow[num_gyro_average_windows];
    private readonly Stopwatch gyroAverageTimer = new();
    private readonly SixAxis now;
    private readonly SixAxis sPrev;

    private bool calibrationDone;
    private double gyro_accel_magnitude = 1.0f;
    private int gyro_average_window_front_index;
    private int gyro_offset_x;
    private int gyro_offset_y;
    private int gyro_offset_z;

    private int temInt;

    public DS4SixAxis()
    {
        sPrev = new SixAxis(0, 0, 0, 0, 0, 0, 0.0);
        now = new SixAxis(0, 0, 0, 0, 0, 0, 0.0);
        StartContinuousCalibration();
    }

    public long CntCalibrating => gyroAverageTimer.IsRunning ? gyroAverageTimer.ElapsedMilliseconds : 0;

    //public event EventHandler<SixAxisEventArgs> SixAccelMoved = null;
    public event SixAxisHandler<SixAxisEventArgs> SixAccelMoved;

    public void SetCalibrationData(ref byte[] calibData, bool useAltGyroCalib)
    {
        int pitchPlus,
            pitchMinus,
            yawPlus,
            yawMinus,
            rollPlus,
            rollMinus,
            accelXPlus,
            accelXMinus,
            accelYPlus,
            accelYMinus,
            accelZPlus,
            accelZMinus,
            gyroSpeedPlus,
            gyroSpeedMinus;

        calibrationData[0].bias = (short)((ushort)(calibData[2] << 8) | calibData[1]);
        calibrationData[1].bias = (short)((ushort)(calibData[4] << 8) | calibData[3]);
        calibrationData[2].bias = (short)((ushort)(calibData[6] << 8) | calibData[5]);

        if (!useAltGyroCalib)
        {
            pitchPlus = temInt = (short)((ushort)(calibData[8] << 8) | calibData[7]);
            yawPlus = temInt = (short)((ushort)(calibData[10] << 8) | calibData[9]);
            rollPlus = temInt = (short)((ushort)(calibData[12] << 8) | calibData[11]);
            pitchMinus = temInt = (short)((ushort)(calibData[14] << 8) | calibData[13]);
            yawMinus = temInt = (short)((ushort)(calibData[16] << 8) | calibData[15]);
            rollMinus = temInt = (short)((ushort)(calibData[18] << 8) | calibData[17]);
        }
        else
        {
            pitchPlus = temInt = (short)((ushort)(calibData[8] << 8) | calibData[7]);
            pitchMinus = temInt = (short)((ushort)(calibData[10] << 8) | calibData[9]);
            yawPlus = temInt = (short)((ushort)(calibData[12] << 8) | calibData[11]);
            yawMinus = temInt = (short)((ushort)(calibData[14] << 8) | calibData[13]);
            rollPlus = temInt = (short)((ushort)(calibData[16] << 8) | calibData[15]);
            rollMinus = temInt = (short)((ushort)(calibData[18] << 8) | calibData[17]);
        }

        gyroSpeedPlus = temInt = (short)((ushort)(calibData[20] << 8) | calibData[19]);
        gyroSpeedMinus = temInt = (short)((ushort)(calibData[22] << 8) | calibData[21]);
        accelXPlus = temInt = (short)((ushort)(calibData[24] << 8) | calibData[23]);
        accelXMinus = temInt = (short)((ushort)(calibData[26] << 8) | calibData[25]);

        accelYPlus = temInt = (short)((ushort)(calibData[28] << 8) | calibData[27]);
        accelYMinus = temInt = (short)((ushort)(calibData[30] << 8) | calibData[29]);

        accelZPlus = temInt = (short)((ushort)(calibData[32] << 8) | calibData[31]);
        accelZMinus = temInt = (short)((ushort)(calibData[34] << 8) | calibData[33]);

        var gyroSpeed2x = temInt = gyroSpeedPlus + gyroSpeedMinus;
        calibrationData[0].sensNumer = gyroSpeed2x * SixAxis.GYRO_RES_IN_DEG_SEC;
        calibrationData[0].sensDenom = pitchPlus - pitchMinus;

        calibrationData[1].sensNumer = gyroSpeed2x * SixAxis.GYRO_RES_IN_DEG_SEC;
        calibrationData[1].sensDenom = yawPlus - yawMinus;

        calibrationData[2].sensNumer = gyroSpeed2x * SixAxis.GYRO_RES_IN_DEG_SEC;
        calibrationData[2].sensDenom = rollPlus - rollMinus;

        var accelRange = temInt = accelXPlus - accelXMinus;
        calibrationData[3].bias = accelXPlus - accelRange / 2;
        calibrationData[3].sensNumer = 2 * SixAxis.ACC_RES_PER_G;
        calibrationData[3].sensDenom = accelRange;

        accelRange = temInt = accelYPlus - accelYMinus;
        calibrationData[4].bias = accelYPlus - accelRange / 2;
        calibrationData[4].sensNumer = 2 * SixAxis.ACC_RES_PER_G;
        calibrationData[4].sensDenom = accelRange;

        accelRange = temInt = accelZPlus - accelZMinus;
        calibrationData[5].bias = accelZPlus - accelRange / 2;
        calibrationData[5].sensNumer = 2 * SixAxis.ACC_RES_PER_G;
        calibrationData[5].sensDenom = accelRange;

        // Check that denom will not be zero.
        calibrationDone = calibrationData[0].sensDenom != 0 &&
                          calibrationData[1].sensDenom != 0 &&
                          calibrationData[2].sensDenom != 0 &&
                          accelRange != 0;
    }

    private void ApplyCalibrations(ref int yaw, ref int pitch, ref int roll,
        ref int accelX, ref int accelY, ref int accelZ)
    {
        var current = calibrationData[0];
        temInt = pitch - current.bias;
        pitch = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

        current = calibrationData[1];
        temInt = yaw - current.bias;
        yaw = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

        current = calibrationData[2];
        temInt = roll - current.bias;
        roll = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

        current = calibrationData[3];
        temInt = accelX - current.bias;
        accelX = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

        current = calibrationData[4];
        temInt = accelY - current.bias;
        accelY = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

        current = calibrationData[5];
        temInt = accelZ - current.bias;
        accelZ = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));
    }

    public unsafe void HandleSixAxis(byte* gyro, byte* accel, DS4State state,
        double elapsedDelta)
    {
        unchecked
        {
            int currentYaw = (short)((ushort)(gyro[3] << 8) | gyro[2]);
            int currentPitch = (short)((ushort)(gyro[1] << 8) | gyro[0]);
            int currentRoll = (short)((ushort)(gyro[5] << 8) | gyro[4]);
            int AccelX = (short)((ushort)(accel[1] << 8) | accel[0]);
            int AccelY = (short)((ushort)(accel[3] << 8) | accel[2]);
            int AccelZ = (short)((ushort)(accel[5] << 8) | accel[4]);

            //Console.WriteLine("AccelZ: {0}", AccelZ);

            if (calibrationDone)
                ApplyCalibrations(ref currentYaw, ref currentPitch, ref currentRoll, ref AccelX, ref AccelY,
                    ref AccelZ);

            if (gyroAverageTimer.IsRunning)
                CalcSensorCamples(ref currentYaw, ref currentPitch, ref currentRoll, ref AccelX, ref AccelY,
                    ref AccelZ);

            currentYaw -= gyro_offset_x;
            currentPitch -= gyro_offset_y;
            currentRoll -= gyro_offset_z;

            SixAxisEventArgs args = null;
            if (AccelX != 0 || AccelY != 0 || AccelZ != 0)
                if (SixAccelMoved != null)
                {
                    sPrev.CopyFrom(now);
                    now.Populate(currentYaw, currentPitch, currentRoll,
                        AccelX, AccelY, AccelZ, elapsedDelta, sPrev);

                    args = new SixAxisEventArgs(state.ReportTimeStamp, now);
                    state.Motion = now;
                    SixAccelMoved(this, args);
                }
        }
    }

    // Entry point to run continuous calibration for non-DS4 input devices
    public void PrepareNonDS4SixAxis(ref int currentYaw, ref int currentPitch, ref int currentRoll,
        ref int AccelX, ref int AccelY, ref int AccelZ)
    {
        unchecked
        {
            if (gyroAverageTimer.IsRunning)
                CalcSensorCamples(ref currentYaw, ref currentPitch, ref currentRoll, ref AccelX, ref AccelY,
                    ref AccelZ);

            currentYaw -= gyro_offset_x;
            currentPitch -= gyro_offset_y;
            currentRoll -= gyro_offset_z;
        }
    }

    private void CalcSensorCamples(ref int currentYaw, ref int currentPitch, ref int currentRoll, ref int AccelX,
        ref int AccelY, ref int AccelZ)
    {
        unchecked
        {
            var accelMag = Math.Sqrt(AccelX * AccelX + AccelY * AccelY + AccelZ * AccelZ);
            PushSensorSamples(currentYaw, currentPitch, currentRoll, (float)accelMag);
            if (gyroAverageTimer.ElapsedMilliseconds > 5000L)
            {
                gyroAverageTimer.Stop();
                AverageGyro(ref gyro_offset_x, ref gyro_offset_y, ref gyro_offset_z, ref gyro_accel_magnitude);
#if DEBUG
                Console.WriteLine("AverageGyro {0} {1} {2} {3}", gyro_offset_x, gyro_offset_y, gyro_offset_z,
                    gyro_accel_magnitude);
#endif
            }
        }
    }

    public bool fixupInvertedGyroAxis()
    {
        var result = false;
        // Some, not all, DS4 rev1 gamepads have an inverted YAW gyro axis calibration value (sensNumber>0 but at the same time sensDenom value is <0 while other two axies have both attributes >0).
        // If this gamepad has YAW calibration with weird mixed values then fix it automatically to workaround inverted YAW axis problem.
        if (calibrationData[1].sensNumer > 0 && calibrationData[1].sensDenom < 0 &&
            calibrationData[0].sensDenom > 0 && calibrationData[2].sensDenom > 0)
        {
            calibrationData[1].sensDenom *= -1;
            result = true; // Fixed inverted axis
        }

        return result;
    }

    public void FireSixAxisEvent(SixAxisEventArgs args)
    {
        SixAccelMoved?.Invoke(this, args);
    }

    public void StartContinuousCalibration()
    {
        for (var i = 0; i < gyro_average_window.Length; i++) gyro_average_window[i] = new GyroAverageWindow();
        gyroAverageTimer.Start();
    }

    public void StopContinuousCalibration()
    {
        gyroAverageTimer.Stop();
        gyroAverageTimer.Reset();
        for (var i = 0; i < gyro_average_window.Length; i++) gyro_average_window[i].Reset();
    }

    public void ResetContinuousCalibration()
    {
        // Potential race condition with CalcSensorCamples() since this method is called after checking gyroAverageTimer.IsRunning == true
        StopContinuousCalibration();
        StartContinuousCalibration();
    }

    public void PushSensorSamples(int x, int y, int z, double accelMagnitude)
    {
        // push samples
        var windowPointer = gyro_average_window[gyro_average_window_front_index];

        if (windowPointer.StopIfElapsed(gyro_average_window_ms))
        {
            Console.WriteLine("GyroAvg[{0}], numSamples: {1}", gyro_average_window_front_index,
                windowPointer.numSamples);

            // next
            gyro_average_window_front_index = (gyro_average_window_front_index + num_gyro_average_windows - 1) %
                                              num_gyro_average_windows;
            windowPointer = gyro_average_window[gyro_average_window_front_index];
            windowPointer.Reset();
        }

        // accumulate
        windowPointer.numSamples++;
        windowPointer.x += x;
        windowPointer.y += y;
        windowPointer.z += z;
        windowPointer.accelMagnitude += accelMagnitude;
    }

    public void AverageGyro(ref int x, ref int y, ref int z, ref double accelMagnitude)
    {
        var weight = 0.0;
        var totalX = 0.0;
        var totalY = 0.0;
        var totalZ = 0.0;
        var totalAccelMagnitude = 0.0;

        var wantedMs = 5000;
        for (var i = 0; i < num_gyro_average_windows && wantedMs > 0; i++)
        {
            var cycledIndex = (i + gyro_average_window_front_index) % num_gyro_average_windows;
            var windowPointer = gyro_average_window[cycledIndex];
            if (windowPointer.numSamples == 0 || windowPointer.DurationMs == 0) continue;

            double thisWeight;
            double fNumSamples = windowPointer.numSamples;
            if (wantedMs < windowPointer.DurationMs)
            {
                thisWeight = (float)wantedMs / windowPointer.DurationMs;
                wantedMs = 0;
            }
            else
            {
                thisWeight = windowPointer.GetWeight(gyro_average_window_ms);
                wantedMs -= windowPointer.DurationMs;
            }

            totalX += windowPointer.x / fNumSamples * thisWeight;
            totalY += windowPointer.y / fNumSamples * thisWeight;
            totalZ += windowPointer.z / fNumSamples * thisWeight;
            totalAccelMagnitude += windowPointer.accelMagnitude / fNumSamples * thisWeight;
            weight += thisWeight;
        }

        if (weight > 0.0)
        {
            x = (int)(totalX / weight);
            y = (int)(totalY / weight);
            z = (int)(totalZ / weight);
            accelMagnitude = totalAccelMagnitude / weight;
        }
    }
}