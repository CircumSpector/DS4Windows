using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows
{
    public partial class Global
    {
        /// <summary>
        ///     Loading and Saving decimal values in configuration files should always use en-US decimal format (ie. dot char as
        ///     decimal separator char, not comma char)
        /// </summary>
        public static CultureInfo ConfigFileDecimalCulture => new("en-US");

        /// <summary>
        ///     Full path to main executable.
        /// </summary>
        public static string ExecutableLocation => Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        ///     Directory containing the <see cref="ExecutableFileName" />.
        /// </summary>
        public static string ExecutableDirectory => Directory.GetParent(ExecutableLocation).FullName;

        /// <summary>
        ///     File name of main executable.
        /// </summary>
        public static string ExecutableFileName => Path.GetFileName(ExecutableLocation);

        /// <summary>
        ///     <see cref="FileVersionInfo" /> of <see cref="ExecutableLocation" />.
        /// </summary>
        public static FileVersionInfo ExecutableFileVersion => FileVersionInfo.GetVersionInfo(ExecutableLocation);

        /// <summary>
        ///     Product version of <see cref="ExecutableFileVersion" />.
        /// </summary>
        public static string ExecutableProductVersion => ExecutableFileVersion.ProductVersion;

        /// <summary>
        ///     Numeric representation of <see cref="ExecutableFileVersion" />.
        /// </summary>
        public static ulong ExecutableVersionLong => ((ulong)ExecutableFileVersion.ProductMajorPart << 48) |
                                                     ((ulong)ExecutableFileVersion.ProductMinorPart << 32) |
                                                     ((ulong)ExecutableFileVersion.ProductBuildPart << 16);

        /// <summary>
        ///     Absolute path to roaming application directory in current user profile.
        /// </summary>
        public static string RoamingAppDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.ApplicationName);

        /// <summary>
        ///     Is the underlying OS Windows 8 (or newer).
        /// </summary>
        public static bool IsWin8OrGreater
        {
            get
            {
                var result = false;

                switch (Environment.OSVersion.Version.Major)
                {
                    case > 6:
                    case 6 when Environment.OSVersion.Version.Minor >= 2:
                        result = true;
                        break;
                }

                return result;
            }
        }

        /// <summary>
        ///     Is the underlying OS Windows 10 (or newer).
        /// </summary>
        public static bool IsWin10OrGreater => Environment.OSVersion.Version.Major >= 10;

        public static string RuntimeAppDataPath { get; set; } = RoamingAppDataPath;

        /// <summary>
        ///     Check if Admin Rights are needed to write in Application Directory
        /// </summary>
        /// <value></value>
        public static bool IsAdminNeeded
        {
            get
            {
                try
                {
                    using (var fs = File.Create(
                        Path.Combine(
                            ExecutableDirectory,
                            Path.GetRandomFileName()
                        ),
                        1,
                        FileOptions.DeleteOnClose)
                    )
                    {
                    }

                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        /// <summary>
        ///     Check if the current user has elevated privileges.
        /// </summary>
        public static bool IsAdministrator
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        ///     Used to hold device type desired from Profile Editor
        /// </summary>
        public static OutputDeviceType[] OutDevTypeTemp { get; set; } = new OutputDeviceType[TEST_PROFILE_ITEM_COUNT]
        {
            OutputDeviceType.Xbox360Controller, OutputDeviceType.Xbox360Controller,
            OutputDeviceType.Xbox360Controller, OutputDeviceType.Xbox360Controller,
            OutputDeviceType.Xbox360Controller, OutputDeviceType.Xbox360Controller,
            OutputDeviceType.Xbox360Controller, OutputDeviceType.Xbox360Controller,
            OutputDeviceType.Xbox360Controller
        };

        /// <summary>
        ///     Used to hold the currently active controller output type in use for a slot.
        /// </summary>
        public static OutputDeviceType[] ActiveOutDevType { get; set; } = new OutputDeviceType[TEST_PROFILE_ITEM_COUNT]
        {
            OutputDeviceType.None, OutputDeviceType.None,
            OutputDeviceType.None, OutputDeviceType.None,
            OutputDeviceType.None, OutputDeviceType.None,
            OutputDeviceType.None, OutputDeviceType.None,
            OutputDeviceType.None
        };

        public static X360Controls[] DefaultButtonMapping =
        {
            X360Controls.None, // DS4Controls.None
            X360Controls.LXNeg, // DS4Controls.LXNeg
            X360Controls.LXPos, // DS4Controls.LXPos
            X360Controls.LYNeg, // DS4Controls.LYNeg
            X360Controls.LYPos, // DS4Controls.LYPos
            X360Controls.RXNeg, // DS4Controls.RXNeg
            X360Controls.RXPos, // DS4Controls.RXPos
            X360Controls.RYNeg, // DS4Controls.RYNeg
            X360Controls.RYPos, // DS4Controls.RYPos
            X360Controls.LB, // DS4Controls.L1
            X360Controls.LT, // DS4Controls.L2
            X360Controls.LS, // DS4Controls.L3
            X360Controls.RB, // DS4Controls.R1
            X360Controls.RT, // DS4Controls.R2
            X360Controls.RS, // DS4Controls.R3
            X360Controls.X, // DS4Controls.Square
            X360Controls.Y, // DS4Controls.Triangle
            X360Controls.B, // DS4Controls.Circle
            X360Controls.A, // DS4Controls.Cross
            X360Controls.DpadUp, // DS4Controls.DpadUp
            X360Controls.DpadRight, // DS4Controls.DpadRight
            X360Controls.DpadDown, // DS4Controls.DpadDown
            X360Controls.DpadLeft, // DS4Controls.DpadLeft
            X360Controls.Guide, // DS4Controls.PS
            X360Controls.LeftMouse, // DS4Controls.TouchLeft
            X360Controls.MiddleMouse, // DS4Controls.TouchUpper
            X360Controls.RightMouse, // DS4Controls.TouchMulti
            X360Controls.LeftMouse, // DS4Controls.TouchRight
            X360Controls.Back, // DS4Controls.Share
            X360Controls.Start, // DS4Controls.Options
            X360Controls.None, // DS4Controls.Mute
            X360Controls.None, // DS4Controls.GyroXPos
            X360Controls.None, // DS4Controls.GyroXNeg
            X360Controls.None, // DS4Controls.GyroZPos
            X360Controls.None, // DS4Controls.GyroZNeg
            X360Controls.None, // DS4Controls.SwipeLeft
            X360Controls.None, // DS4Controls.SwipeRight
            X360Controls.None, // DS4Controls.SwipeUp
            X360Controls.None, // DS4Controls.SwipeDown
            X360Controls.None, // DS4Controls.L2FullPull
            X360Controls.None, // DS4Controls.R2FullPull
            X360Controls.None, // DS4Controls.GyroSwipeLeft
            X360Controls.None, // DS4Controls.GyroSwipeRight
            X360Controls.None, // DS4Controls.GyroSwipeUp
            X360Controls.None, // DS4Controls.GyroSwipeDown
            X360Controls.None, // DS4Controls.Capture
            X360Controls.None, // DS4Controls.SideL
            X360Controls.None, // DS4Controls.SideR
            X360Controls.None, // DS4Controls.LSOuter
            X360Controls.None // DS4Controls.RSOuter
        };

         public static Dictionary<X360Controls, string> XboxDefaultNames =>
            new()
            {
                [X360Controls.LXNeg] = "Left X-Axis-",
                [X360Controls.LXPos] = "Left X-Axis+",
                [X360Controls.LYNeg] = "Left Y-Axis-",
                [X360Controls.LYPos] = "Left Y-Axis+",
                [X360Controls.RXNeg] = "Right X-Axis-",
                [X360Controls.RXPos] = "Right X-Axis+",
                [X360Controls.RYNeg] = "Right Y-Axis-",
                [X360Controls.RYPos] = "Right Y-Axis+",
                [X360Controls.LB] = "Left Bumper",
                [X360Controls.LT] = "Left Trigger",
                [X360Controls.LS] = "Left Stick",
                [X360Controls.RB] = "Right Bumper",
                [X360Controls.RT] = "Right Trigger",
                [X360Controls.RS] = "Right Stick",
                [X360Controls.X] = "X Button",
                [X360Controls.Y] = "Y Button",
                [X360Controls.B] = "B Button",
                [X360Controls.A] = "A Button",
                [X360Controls.DpadUp] = "Up Button",
                [X360Controls.DpadRight] = "Right Button",
                [X360Controls.DpadDown] = "Down Button",
                [X360Controls.DpadLeft] = "Left Button",
                [X360Controls.Guide] = "Guide",
                [X360Controls.Back] = "Back",
                [X360Controls.Start] = "Start",
                [X360Controls.TouchpadClick] = "Touchpad Click",
                [X360Controls.LeftMouse] = "Left Mouse Button",
                [X360Controls.RightMouse] = "Right Mouse Button",
                [X360Controls.MiddleMouse] = "Middle Mouse Button",
                [X360Controls.FourthMouse] = "4th Mouse Button",
                [X360Controls.FifthMouse] = "5th Mouse Button",
                [X360Controls.WUP] = "Mouse Wheel Up",
                [X360Controls.WDOWN] = "Mouse Wheel Down",
                [X360Controls.MouseUp] = "Mouse Up",
                [X360Controls.MouseDown] = "Mouse Down",
                [X360Controls.MouseLeft] = "Mouse Left",
                [X360Controls.MouseRight] = "Mouse Right",
                [X360Controls.Unbound] = "Unbound",
                [X360Controls.None] = "Unassigned"
            };

        public static Dictionary<X360Controls, string> Ds4DefaultNames => new()
        {
            [X360Controls.LXNeg] = "Left X-Axis-",
            [X360Controls.LXPos] = "Left X-Axis+",
            [X360Controls.LYNeg] = "Left Y-Axis-",
            [X360Controls.LYPos] = "Left Y-Axis+",
            [X360Controls.RXNeg] = "Right X-Axis-",
            [X360Controls.RXPos] = "Right X-Axis+",
            [X360Controls.RYNeg] = "Right Y-Axis-",
            [X360Controls.RYPos] = "Right Y-Axis+",
            [X360Controls.LB] = "L1",
            [X360Controls.LT] = "L2",
            [X360Controls.LS] = "L3",
            [X360Controls.RB] = "R1",
            [X360Controls.RT] = "R2",
            [X360Controls.RS] = "R3",
            [X360Controls.X] = "Square",
            [X360Controls.Y] = "Triangle",
            [X360Controls.B] = "Circle",
            [X360Controls.A] = "Cross",
            [X360Controls.DpadUp] = "Dpad Up",
            [X360Controls.DpadRight] = "Dpad Right",
            [X360Controls.DpadDown] = "Dpad Down",
            [X360Controls.DpadLeft] = "Dpad Left",
            [X360Controls.Guide] = "PS",
            [X360Controls.Back] = "Share",
            [X360Controls.Start] = "Options",
            [X360Controls.TouchpadClick] = "Touchpad Click",
            [X360Controls.LeftMouse] = "Left Mouse Button",
            [X360Controls.RightMouse] = "Right Mouse Button",
            [X360Controls.MiddleMouse] = "Middle Mouse Button",
            [X360Controls.FourthMouse] = "4th Mouse Button",
            [X360Controls.FifthMouse] = "5th Mouse Button",
            [X360Controls.WUP] = "Mouse Wheel Up",
            [X360Controls.WDOWN] = "Mouse Wheel Down",
            [X360Controls.MouseUp] = "Mouse Up",
            [X360Controls.MouseDown] = "Mouse Down",
            [X360Controls.MouseLeft] = "Mouse Left",
            [X360Controls.MouseRight] = "Mouse Right",
            [X360Controls.Unbound] = "Unbound"
        };

        public static Dictionary<DS4ControlItem, string> Ds4InputNames => new()
        {
            [DS4ControlItem.LXNeg] = "Left X-Axis-",
            [DS4ControlItem.LXPos] = "Left X-Axis+",
            [DS4ControlItem.LYNeg] = "Left Y-Axis-",
            [DS4ControlItem.LYPos] = "Left Y-Axis+",
            [DS4ControlItem.RXNeg] = "Right X-Axis-",
            [DS4ControlItem.RXPos] = "Right X-Axis+",
            [DS4ControlItem.RYNeg] = "Right Y-Axis-",
            [DS4ControlItem.RYPos] = "Right Y-Axis+",
            [DS4ControlItem.L1] = "L1",
            [DS4ControlItem.L2] = "L2",
            [DS4ControlItem.L3] = "L3",
            [DS4ControlItem.R1] = "R1",
            [DS4ControlItem.R2] = "R2",
            [DS4ControlItem.R3] = "R3",
            [DS4ControlItem.Square] = "Square",
            [DS4ControlItem.Triangle] = "Triangle",
            [DS4ControlItem.Circle] = "Circle",
            [DS4ControlItem.Cross] = "Cross",
            [DS4ControlItem.DpadUp] = "Dpad Up",
            [DS4ControlItem.DpadRight] = "Dpad Right",
            [DS4ControlItem.DpadDown] = "Dpad Down",
            [DS4ControlItem.DpadLeft] = "Dpad Left",
            [DS4ControlItem.PS] = "PS",
            [DS4ControlItem.Share] = "Share",
            [DS4ControlItem.Options] = "Options",
            [DS4ControlItem.Mute] = "Mute",
            [DS4ControlItem.Capture] = "Capture",
            [DS4ControlItem.SideL] = "Side L",
            [DS4ControlItem.SideR] = "Side R",
            [DS4ControlItem.TouchLeft] = "Left Touch",
            [DS4ControlItem.TouchUpper] = "Upper Touch",
            [DS4ControlItem.TouchMulti] = "Multitouch",
            [DS4ControlItem.TouchRight] = "Right Touch",
            [DS4ControlItem.GyroXPos] = "Gyro X+",
            [DS4ControlItem.GyroXNeg] = "Gyro X-",
            [DS4ControlItem.GyroZPos] = "Gyro Z+",
            [DS4ControlItem.GyroZNeg] = "Gyro Z-",
            [DS4ControlItem.SwipeLeft] = "Swipe Left",
            [DS4ControlItem.SwipeRight] = "Swipe Right",
            [DS4ControlItem.SwipeUp] = "Swipe Up",
            [DS4ControlItem.SwipeDown] = "Swipe Down",
            [DS4ControlItem.L2FullPull] = "L2 Full Pull",
            [DS4ControlItem.R2FullPull] = "R2 Full Pull",
            [DS4ControlItem.LSOuter] = "LS Outer",
            [DS4ControlItem.RSOuter] = "RS Outer",

            [DS4ControlItem.GyroSwipeLeft] = "Gyro Swipe Left",
            [DS4ControlItem.GyroSwipeRight] = "Gyro Swipe Right",
            [DS4ControlItem.GyroSwipeUp] = "Gyro Swipe Up",
            [DS4ControlItem.GyroSwipeDown] = "Gyro Swipe Down"
        };

        public static Dictionary<DS4ControlItem, int> MacroDs4Values => new()
        {
            [DS4ControlItem.Cross] = 261, [DS4ControlItem.Circle] = 262,
            [DS4ControlItem.Square] = 263, [DS4ControlItem.Triangle] = 264,
            [DS4ControlItem.Options] = 265, [DS4ControlItem.Share] = 266,
            [DS4ControlItem.DpadUp] = 267, [DS4ControlItem.DpadDown] = 268,
            [DS4ControlItem.DpadLeft] = 269, [DS4ControlItem.DpadRight] = 270,
            [DS4ControlItem.PS] = 271, [DS4ControlItem.L1] = 272,
            [DS4ControlItem.R1] = 273, [DS4ControlItem.L2] = 274,
            [DS4ControlItem.R2] = 275, [DS4ControlItem.L3] = 276,
            [DS4ControlItem.R3] = 277, [DS4ControlItem.LXPos] = 278,
            [DS4ControlItem.LXNeg] = 279, [DS4ControlItem.LYPos] = 280,
            [DS4ControlItem.LYNeg] = 281, [DS4ControlItem.RXPos] = 282,
            [DS4ControlItem.RXNeg] = 283, [DS4ControlItem.RYPos] = 284,
            [DS4ControlItem.RYNeg] = 285,
            [DS4ControlItem.TouchLeft] = 286, [DS4ControlItem.TouchRight] = 286,
            [DS4ControlItem.TouchUpper] = 286, [DS4ControlItem.TouchMulti] = 286
        };

        // Create mapping array at runtime
        public static DS4ControlItem[] ReverseX360ButtonMapping = new Func<DS4ControlItem[]>(() =>
        {
            var temp = new DS4ControlItem[DefaultButtonMapping.Length];
            for (int i = 0, arlen = DefaultButtonMapping.Length; i < arlen; i++)
            {
                var mapping = DefaultButtonMapping[i];
                if (mapping != X360Controls.None) temp[(int)mapping] = (DS4ControlItem)i;
            }

            return temp;
        })();
    }
}