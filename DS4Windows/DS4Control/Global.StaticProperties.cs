using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;

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

        public static bool IsAdministrator
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}