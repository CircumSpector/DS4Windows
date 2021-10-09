using System;
using System.Diagnostics;
using System.Drawing;
using DS4WinWPF.DS4Control.IoC.Services;
using OpenTracing.Util;
using static System.Math;
using static DS4Windows.Global;

namespace DS4Windows
{
    public class DS4LightBar
    {
        internal const int PULSE_FLASH_DURATION = 2000;
        internal const double PULSE_FLASH_SEGMENTS = PULSE_FLASH_DURATION / 40;
        internal const int PULSE_CHARGING_DURATION = 4000;
        internal const double PULSE_CHARGING_SEGMENTS = PULSE_CHARGING_DURATION / 40 - 2;

        private static readonly byte[ /* Light On duration */, /* Light Off duration */] BatteryIndicatorDurations =
        {
            { 28, 252 }, // on 10% of the time at 0
            { 28, 252 },
            { 56, 224 },
            { 84, 196 },
            { 112, 168 },
            { 140, 140 },
            { 168, 112 },
            { 196, 84 },
            { 224, 56 }, // on 80% of the time at 80, etc.
            { 252, 28 }, // on 90% of the time at 90
            { 0, 0 } // use on 100%. 0 is for "charging" OR anything sufficiently-"charged"
        };

        private static readonly double[] counters = new double[MAX_DS4_CONTROLLER_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public static Stopwatch[] fadewatches = new Stopwatch[MAX_DS4_CONTROLLER_COUNT]
        {
            new(), new(), new(), new(),
            new(), new(), new(), new()
        };

        private static readonly bool[] fadedirection = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        private static readonly DateTime[] oldnow = new DateTime[MAX_DS4_CONTROLLER_COUNT]
        {
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow,
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow
        };

        public static bool[] forcelight = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        public static DS4Color[] forcedColor = new DS4Color[MAX_DS4_CONTROLLER_COUNT];
        public static byte[] forcedFlash = new byte[MAX_DS4_CONTROLLER_COUNT];

        public static bool defaultLight = false, shuttingdown = false;

        private static byte ApplyRatio(byte b1, byte b2, double r)
        {
            if (r > 100.0)
                r = 100.0;
            else if (r < 0.0)
                r = 0.0;

            r *= 0.01;
            return (byte)Round(b1 * (1 - r) + b2 * r, 0);
        }

        public static DS4Color GetTransitionedColor(DS4Color c1, DS4Color c2, double ratio)
        {
            return new DS4Color(
                ApplyRatio(c1.Red, c2.Red, ratio), 
                ApplyRatio(c1.Green, c2.Green, ratio),
                ApplyRatio(c1.Blue, c2.Blue, ratio)
                );
        }

        public static void UpdateLightBar(DS4Device device, int deviceNum)
        {

            using var scope = GlobalTracer.Instance.BuildSpan($"{nameof(DS4LightBar)}::{nameof(UpdateLightBar)}")
                .StartActive(true);


            var color = new DS4Color(0, 0, 0);
            var useForceLight = forcelight[deviceNum];
            var lightbarSettingInfo = AppSettingsService.Instance.Settings.LightbarSettingInfo[deviceNum];
            var lightModeInfo = lightbarSettingInfo.Ds4WinSettings;
            var useLightRoutine = lightbarSettingInfo.Mode == LightbarMode.DS4Win;
            //bool useLightRoutine = false;
            if (!defaultLight && !useForceLight && useLightRoutine)
            {
                if (lightModeInfo.UseCustomLed)
                {
                    color = lightModeInfo.LedAsBattery
                        ? GetTransitionedColor(lightModeInfo.LowLed, lightModeInfo.CustomLed, device.GetBattery())
                        : lightModeInfo.CustomLed;
                }
                else
                {
                    var rainbow = lightModeInfo.Rainbow;
                    if (rainbow > 0)
                    {
                        // Display rainbow
                        var now = DateTime.UtcNow;
                        if (now >= oldnow[deviceNum] +
                            TimeSpan.FromMilliseconds(10)) //update by the millisecond that way it's a smooth transtion
                        {
                            oldnow[deviceNum] = now;
                            if (device.IsCharging())
                                counters[deviceNum] -= 1.5 * 3 / rainbow;
                            else
                                counters[deviceNum] += 1.5 * 3 / rainbow;
                        }

                        if (counters[deviceNum] < 0)
                            counters[deviceNum] = 180000;
                        else if (counters[deviceNum] > 180000)
                            counters[deviceNum] = 0;

                        var maxSat = lightModeInfo.MaxRainbowSaturation; // GetMaxSatRainbow(deviceNum);
                        if (lightModeInfo.LedAsBattery)
                        {
                            var useSat = (byte)(maxSat == 1.0
                                ? device.GetBattery() * 2.55
                                : device.GetBattery() * 2.55 * maxSat);
                            color = HuetoRGB((float)counters[deviceNum] % 360, useSat);
                        }
                        else
                        {
                            color = HuetoRGB((float)counters[deviceNum] % 360,
                                (byte)(maxSat == 1.0 ? 255 : 255 * maxSat));
                        }
                    }
                    else if (lightModeInfo.LedAsBattery)
                    {
                        color = GetTransitionedColor(lightModeInfo.LowLed, lightModeInfo.Led, device.GetBattery());
                    }
                    else
                    {
                        color = AppSettingsService.Instance.Settings.LightbarSettingInfo[deviceNum].Ds4WinSettings.Led;
                    }
                }

                if (device.GetBattery() <= lightModeInfo.FlashAt && !defaultLight && !device.IsCharging())
                {
                    var flashColor = lightModeInfo.FlashLed;
                    if (!(flashColor.Red == 0 &&
                          flashColor.Green == 0 &&
                          flashColor.Blue == 0))
                        color = lightModeInfo.FlashLed;

                    if (lightModeInfo.FlashType == 1)
                    {
                        var ratio = 0.0;

                        if (!fadewatches[deviceNum].IsRunning)
                        {
                            var temp = fadedirection[deviceNum];
                            fadedirection[deviceNum] = !temp;
                            fadewatches[deviceNum].Restart();
                            ratio = temp ? 100.0 : 0.0;
                        }
                        else
                        {
                            var elapsed = fadewatches[deviceNum].ElapsedMilliseconds;

                            if (fadedirection[deviceNum])
                            {
                                if (elapsed < PULSE_FLASH_DURATION)
                                {
                                    elapsed = elapsed / 40;
                                    ratio = 100.0 * (elapsed / PULSE_FLASH_SEGMENTS);
                                }
                                else
                                {
                                    ratio = 100.0;
                                    fadewatches[deviceNum].Stop();
                                }
                            }
                            else
                            {
                                if (elapsed < PULSE_FLASH_DURATION)
                                {
                                    elapsed = elapsed / 40;
                                    ratio = (0 - 100.0) * (elapsed / PULSE_FLASH_SEGMENTS) + 100.0;
                                }
                                else
                                {
                                    ratio = 0.0;
                                    fadewatches[deviceNum].Stop();
                                }
                            }
                        }

                        var tempCol = new DS4Color(0, 0, 0);
                        color = GetTransitionedColor(color, tempCol, ratio);
                    }
                }

                var idleDisconnectTimeout = Instance.Config.GetIdleDisconnectTimeout(deviceNum);
                if (idleDisconnectTimeout > 0 && lightModeInfo.LedAsBattery &&
                    (!device.IsCharging() || device.GetBattery() >= 100))
                {
                    // Fade lightbar by idle time
                    var timeratio = new TimeSpan(DateTime.UtcNow.Ticks - device.lastActive.Ticks);
                    var botratio = timeratio.TotalMilliseconds;
                    var topratio = TimeSpan.FromSeconds(idleDisconnectTimeout).TotalMilliseconds;
                    double ratio = 100.0 * (botratio / topratio), elapsed = ratio;
                    if (ratio >= 50.0 && ratio < 100.0)
                    {
                        var emptyCol = new DS4Color(0, 0, 0);
                        color = GetTransitionedColor(color, emptyCol,
                            (uint)(-100.0 * (elapsed = 0.02 * (ratio - 50.0)) * (elapsed - 2.0)));
                    }
                    else if (ratio >= 100.0)
                    {
                        var emptyCol = new DS4Color(0, 0, 0);
                        color = GetTransitionedColor(color, emptyCol, 100.0);
                    }
                }

                if (device.IsCharging() && device.GetBattery() < 100)
                    switch (lightModeInfo.ChargingType)
                    {
                        case 1:
                        {
                            var ratio = 0.0;

                            if (!fadewatches[deviceNum].IsRunning)
                            {
                                var temp = fadedirection[deviceNum];
                                fadedirection[deviceNum] = !temp;
                                fadewatches[deviceNum].Restart();
                                ratio = temp ? 100.0 : 0.0;
                            }
                            else
                            {
                                var elapsed = fadewatches[deviceNum].ElapsedMilliseconds;

                                if (fadedirection[deviceNum])
                                {
                                    if (elapsed < PULSE_CHARGING_DURATION)
                                    {
                                        elapsed = elapsed / 40;
                                        if (elapsed > PULSE_CHARGING_SEGMENTS)
                                            elapsed = (long)PULSE_CHARGING_SEGMENTS;
                                        ratio = 100.0 * (elapsed / PULSE_CHARGING_SEGMENTS);
                                    }
                                    else
                                    {
                                        ratio = 100.0;
                                        fadewatches[deviceNum].Stop();
                                    }
                                }
                                else
                                {
                                    if (elapsed < PULSE_CHARGING_DURATION)
                                    {
                                        elapsed = elapsed / 40;
                                        if (elapsed > PULSE_CHARGING_SEGMENTS)
                                            elapsed = (long)PULSE_CHARGING_SEGMENTS;
                                        ratio = (0 - 100.0) * (elapsed / PULSE_CHARGING_SEGMENTS) + 100.0;
                                    }
                                    else
                                    {
                                        ratio = 0.0;
                                        fadewatches[deviceNum].Stop();
                                    }
                                }
                            }

                            var emptyCol = new DS4Color(0, 0, 0);
                            color = GetTransitionedColor(color, emptyCol, ratio);
                            break;
                        }
                        case 2:
                        {
                            counters[deviceNum] += 0.167;
                            color = HuetoRGB((float)counters[deviceNum] % 360, 255);
                            break;
                        }
                        case 3:
                        {
                            color = lightModeInfo.ChargingLed; //getChargingColor(deviceNum);
                            break;
                        }
                    }
            }
            else if (useForceLight)
            {
                color = forcedColor[deviceNum];
                useLightRoutine = true;
            }
            else if (shuttingdown)
            {
                color = new DS4Color(0, 0, 0);
                useLightRoutine = true;
            }
            else if (useLightRoutine)
            {
                if (device.GetConnectionType() == ConnectionType.BT)
                    color = new DS4Color(32, 64, 64);
                else
                    color = new DS4Color(0, 0, 0);
            }

            if (useLightRoutine)
            {
                var distanceprofile = Instance.Config.DistanceProfiles[deviceNum] || TempProfileDistance[deviceNum];
                //distanceprofile = (ProfilePath[deviceNum].ToLower().Contains("distance") || TempProfileNames[deviceNum].ToLower().Contains("distance"));
                if (distanceprofile && !defaultLight)
                {
                    // Thing I did for Distance
                    var rumble = device.getLeftHeavySlowRumble() / 2.55f;
                    var max = Max(color.Red, Max(color.Green, color.Blue));
                    if (device.getLeftHeavySlowRumble() > 100)
                    {
                        var maxCol = new DS4Color(max, max, 0);
                        var redCol = new DS4Color(255, 0, 0);
                        color = GetTransitionedColor(maxCol, redCol, rumble);
                    }
                    else
                    {
                        var maxCol = new DS4Color(max, max, 0);
                        var redCol = new DS4Color(255, 0, 0);
                        var tempCol = GetTransitionedColor(maxCol,
                            redCol, 39.6078f);
                        color = GetTransitionedColor(color, tempCol,
                            device.getLeftHeavySlowRumble());
                    }
                }

                /*DS4HapticState haptics = new DS4HapticState
                {
                    LightBarColor = color
                };
                */
                var lightState = new DS4LightbarState
                {
                    LightBarColor = color
                };

                if (lightState.IsLightBarSet())
                {
                    if (useForceLight && forcedFlash[deviceNum] > 0)
                    {
                        lightState.LightBarFlashDurationOff =
                            lightState.LightBarFlashDurationOn = (byte)(25 - forcedFlash[deviceNum]);
                        lightState.LightBarExplicitlyOff = true;
                    }
                    else if (device.GetBattery() <= lightModeInfo.FlashAt && lightModeInfo.FlashType == 0 &&
                             !defaultLight && !device.IsCharging())
                    {
                        var level = device.GetBattery() / 10;
                        if (level >= 10)
                            level = 10; // all values of >~100% are rendered the same

                        lightState.LightBarFlashDurationOn = BatteryIndicatorDurations[level, 0];
                        lightState.LightBarFlashDurationOff = BatteryIndicatorDurations[level, 1];
                    }
                    else if (distanceprofile && device.getLeftHeavySlowRumble() > 155) //also part of Distance
                    {
                        lightState.LightBarFlashDurationOff = lightState.LightBarFlashDurationOn =
                            (byte)(-device.getLeftHeavySlowRumble() + 265);
                        lightState.LightBarExplicitlyOff = true;
                    }
                    else
                    {
                        //haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 1;
                        lightState.LightBarFlashDurationOff = lightState.LightBarFlashDurationOn = 0;
                        lightState.LightBarExplicitlyOff = true;
                    }
                }
                else
                {
                    lightState.LightBarExplicitlyOff = true;
                }

                var tempLightBarOnDuration = device.getLightBarOnDuration();
                if (tempLightBarOnDuration != lightState.LightBarFlashDurationOn && tempLightBarOnDuration != 1 &&
                    lightState.LightBarFlashDurationOn == 0)
                    lightState.LightBarFlashDurationOff = lightState.LightBarFlashDurationOn = 1;

                device.SetLightbarState(ref lightState);
                //device.SetHapticState(ref haptics);
                //device.pushHapticState(ref haptics);
            }
        }

        public static DS4Color HuetoRGB(float hue, byte sat)
        {
            var C = sat;
            var X = (int)(C * (1 - Abs(hue / 60 % 2 - 1)));
            if (0 <= hue && hue < 60)
                return new DS4Color(C, (byte)X, 0);
            if (60 <= hue && hue < 120)
                return new DS4Color((byte)X, C, 0);
            if (120 <= hue && hue < 180)
                return new DS4Color(0, C, (byte)X);
            if (180 <= hue && hue < 240)
                return new DS4Color(0, (byte)X, C);
            if (240 <= hue && hue < 300)
                return new DS4Color((byte)X, 0, C);
            if (300 <= hue && hue < 360)
                return new DS4Color(C, 0, (byte)X);
            return new DS4Color(Color.Red);
        }
    }
}