using System;

namespace DS4WinWPF
{
    public class StatusLogMsg
    {
        private string message;
        private bool warning;

        public string Message
        {
            get => message;
            set
            {
                if (message == value) return;
                message = value;
                MessageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Warning
        {
            get => warning;
            set
            {
                if (warning == value) return;
                warning = value;
                WarningChanged?.Invoke(this, EventArgs.Empty);
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Color => warning ? "Red" : "#FF696969";

        public event EventHandler MessageChanged;

        public event EventHandler WarningChanged;

        public event EventHandler ColorChanged;
    }
}