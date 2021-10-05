using System;

namespace DS4WinWPF.DS4Control.Logging
{
    public class LogItem
    {
        public DateTime Time { get; set; }

        public string Message { get; set; }

        public bool IsWarning { get; set; }

        public string Color => IsWarning ? "Red" : "Black";
    }
}