using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Server.Controller
{
    public class IsHostRunningChangedMessage : MessageBase
    {
        public const string Name = "IsHostRunningChanged";
        public IsHostRunningChangedMessage(bool isRunning)
        {
            IsRunning = isRunning;
        }
        public bool IsRunning { get; }
        public override string MessageName => Name;
    }
}
