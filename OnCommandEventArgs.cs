using System;

namespace TurboCommand.Package
{
    public class OnCommandEventArgs : EventArgs
    {
        public OnCommandEventArgs(string commandGuid, int commandId, string commandName)
        {
            CommandGuid = commandGuid;
            CommandId = commandId;
            CommandFullName = commandName;
        }

        public string CommandFullName { get; set; }
        public string CommandGuid { get; set; }
        public int  CommandId { get; set; }
    }
}
