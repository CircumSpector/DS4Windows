using System.Collections.Generic;
using DS4Windows.DS4Control;

namespace DS4WinWPF
{
    public interface ICommandLineOptions
    {
        bool StartMinimized { get; }

        bool Stop { get; }

        bool DriverInstall { get; }

        bool ReenableDevice { get; }

        bool RunTask { get; }

        bool Command { get; }

        string DeviceInstanceId { get; }

        string CommandArgs { get; }

        string VirtualKBMHandler { get; }
    }

    public class CommandLineOptions : ICommandLineOptions
    {
        private readonly Dictionary<string, string> errors = new();

        public bool HasErrors => errors.Count > 0;

        public bool StartMinimized { get; private set; }

        public bool Stop { get; private set; }

        public bool DriverInstall { get; private set; }

        public bool ReenableDevice { get; private set; }

        public bool RunTask { get; private set; }

        public bool Command { get; private set; }

        public string DeviceInstanceId { get; private set; }

        public string CommandArgs { get; private set; }

        public string VirtualKBMHandler { get; private set; } = VirtualKBMFactory.DEFAULT_IDENTIFIER;

        public void Parse(string[] args)
        {
            errors.Clear();
            //foreach (string arg in args)
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "driverinstall":
                    case "-driverinstall":
                        DriverInstall = true;
                        break;

                    case "re-enabledevice":
                    case "-re-enabledevice":
                        ReenableDevice = true;
                        if (i + 1 < args.Length) DeviceInstanceId = args[++i];

                        break;

                    case "runtask":
                    case "-runtask":
                        RunTask = true;
                        break;

                    case "-stop":
                        Stop = true;
                        break;

                    case "-m":
                        StartMinimized = true;
                        break;

                    case "command":
                    case "-command":
                        Command = true;
                        if (i + 1 < args.Length)
                        {
                            i++;
                            var temp = args[i];
                            if (temp.Length > 0 && temp.Length <= 256)
                            {
                                CommandArgs = temp;
                            }
                            else
                            {
                                Command = false;
                                errors["Command"] = "Command length is invalid";
                            }
                        }
                        else
                        {
                            errors["Command"] = "Command string not given";
                        }

                        break;
                    case "-virtualkbm":
                        if (i + 1 < args.Length)
                        {
                            i++;
                            var temp = args[i];
                            var valid = VirtualKBMFactory.IsValidHandler(temp);
                            if (valid) VirtualKBMHandler = temp;
                        }

                        break;
                }
            }
        }
    }
}