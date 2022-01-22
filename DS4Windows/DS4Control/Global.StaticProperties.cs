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

        public static X360ControlItem[] DefaultButtonMapping =
        {
            X360ControlItem.None, // DS4Controls.None
            X360ControlItem.LXNeg, // DS4Controls.LXNeg
            X360ControlItem.LXPos, // DS4Controls.LXPos
            X360ControlItem.LYNeg, // DS4Controls.LYNeg
            X360ControlItem.LYPos, // DS4Controls.LYPos
            X360ControlItem.RXNeg, // DS4Controls.RXNeg
            X360ControlItem.RXPos, // DS4Controls.RXPos
            X360ControlItem.RYNeg, // DS4Controls.RYNeg
            X360ControlItem.RYPos, // DS4Controls.RYPos
            X360ControlItem.LB, // DS4Controls.L1
            X360ControlItem.LT, // DS4Controls.L2
            X360ControlItem.LS, // DS4Controls.L3
            X360ControlItem.RB, // DS4Controls.R1
            X360ControlItem.RT, // DS4Controls.R2
            X360ControlItem.RS, // DS4Controls.R3
            X360ControlItem.X, // DS4Controls.Square
            X360ControlItem.Y, // DS4Controls.Triangle
            X360ControlItem.B, // DS4Controls.Circle
            X360ControlItem.A, // DS4Controls.Cross
            X360ControlItem.DpadUp, // DS4Controls.DpadUp
            X360ControlItem.DpadRight, // DS4Controls.DpadRight
            X360ControlItem.DpadDown, // DS4Controls.DpadDown
            X360ControlItem.DpadLeft, // DS4Controls.DpadLeft
            X360ControlItem.Guide, // DS4Controls.PS
            X360ControlItem.LeftMouse, // DS4Controls.TouchLeft
            X360ControlItem.MiddleMouse, // DS4Controls.TouchUpper
            X360ControlItem.RightMouse, // DS4Controls.TouchMulti
            X360ControlItem.LeftMouse, // DS4Controls.TouchRight
            X360ControlItem.Back, // DS4Controls.Share
            X360ControlItem.Start, // DS4Controls.Options
            X360ControlItem.None, // DS4Controls.Mute
            X360ControlItem.None, // DS4Controls.GyroXPos
            X360ControlItem.None, // DS4Controls.GyroXNeg
            X360ControlItem.None, // DS4Controls.GyroZPos
            X360ControlItem.None, // DS4Controls.GyroZNeg
            X360ControlItem.None, // DS4Controls.SwipeLeft
            X360ControlItem.None, // DS4Controls.SwipeRight
            X360ControlItem.None, // DS4Controls.SwipeUp
            X360ControlItem.None, // DS4Controls.SwipeDown
            X360ControlItem.None, // DS4Controls.L2FullPull
            X360ControlItem.None, // DS4Controls.R2FullPull
            X360ControlItem.None, // DS4Controls.GyroSwipeLeft
            X360ControlItem.None, // DS4Controls.GyroSwipeRight
            X360ControlItem.None, // DS4Controls.GyroSwipeUp
            X360ControlItem.None, // DS4Controls.GyroSwipeDown
            X360ControlItem.None, // DS4Controls.Capture
            X360ControlItem.None, // DS4Controls.SideL
            X360ControlItem.None, // DS4Controls.SideR
            X360ControlItem.None, // DS4Controls.LSOuter
            X360ControlItem.None // DS4Controls.RSOuter
        };

        public static Dictionary<X360ControlItem, string> Ds4DefaultNames => new()
        {
            [X360ControlItem.LXNeg] = "Left X-Axis-",
            [X360ControlItem.LXPos] = "Left X-Axis+",
            [X360ControlItem.LYNeg] = "Left Y-Axis-",
            [X360ControlItem.LYPos] = "Left Y-Axis+",
            [X360ControlItem.RXNeg] = "Right X-Axis-",
            [X360ControlItem.RXPos] = "Right X-Axis+",
            [X360ControlItem.RYNeg] = "Right Y-Axis-",
            [X360ControlItem.RYPos] = "Right Y-Axis+",
            [X360ControlItem.LB] = "L1",
            [X360ControlItem.LT] = "L2",
            [X360ControlItem.LS] = "L3",
            [X360ControlItem.RB] = "R1",
            [X360ControlItem.RT] = "R2",
            [X360ControlItem.RS] = "R3",
            [X360ControlItem.X] = "Square",
            [X360ControlItem.Y] = "Triangle",
            [X360ControlItem.B] = "Circle",
            [X360ControlItem.A] = "Cross",
            [X360ControlItem.DpadUp] = "Dpad Up",
            [X360ControlItem.DpadRight] = "Dpad Right",
            [X360ControlItem.DpadDown] = "Dpad Down",
            [X360ControlItem.DpadLeft] = "Dpad Left",
            [X360ControlItem.Guide] = "PS",
            [X360ControlItem.Back] = "Share",
            [X360ControlItem.Start] = "Options",
            [X360ControlItem.TouchpadClick] = "Touchpad Click",
            [X360ControlItem.LeftMouse] = "Left Mouse Button",
            [X360ControlItem.RightMouse] = "Right Mouse Button",
            [X360ControlItem.MiddleMouse] = "Middle Mouse Button",
            [X360ControlItem.FourthMouse] = "4th Mouse Button",
            [X360ControlItem.FifthMouse] = "5th Mouse Button",
            [X360ControlItem.WUP] = "Mouse Wheel Up",
            [X360ControlItem.WDOWN] = "Mouse Wheel Down",
            [X360ControlItem.MouseUp] = "Mouse Up",
            [X360ControlItem.MouseDown] = "Mouse Down",
            [X360ControlItem.MouseLeft] = "Mouse Left",
            [X360ControlItem.MouseRight] = "Mouse Right",
            [X360ControlItem.Unbound] = "Unbound"
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
                if (mapping != X360ControlItem.None) temp[(int)mapping] = (DS4ControlItem)i;
            }

            return temp;
        })();
    }
}