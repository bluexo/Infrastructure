//------------------------------------------------------------
//

//
//
//------------------------------------------------------------

using System;

namespace Origine
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandHandler : Attribute
    {
        public string Command { get; set; }
    }

    public class CommandEventArgs : GameEventArgs<CommandEventArgs>
    {
        public string Command { get; set; }
        public object Data { get; set; }
        public object Target { get; set; }

        public CommandEventArgs() { }

        public CommandEventArgs(string cmd, object data)
        {
            Command = cmd;
            Data = data;
        }

        public CommandEventArgs(string cmd, object data, object target) : this(cmd, data)
        {
            Target = target;
        }
    }
}
