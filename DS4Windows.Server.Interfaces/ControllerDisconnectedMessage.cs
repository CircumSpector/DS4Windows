using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Server
{
    public class ControllerDisconnectedMessage : MessageBase
    {
        public const string Name = "ControllerDisconnected";

        public string ControllerDisconnectedId { get; set; }
        public override string MessageName => Name;
    }
}
