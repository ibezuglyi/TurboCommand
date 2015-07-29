using System.Linq;
using Akka.Actor;
using EnvDTE;

namespace TurboCommand.Package
{
    public class CommandSearcher : ReceiveActor
    {
        public CommandSearcher()
        {
            Receive<CommandSearchMessage>(message =>
            {
                var allCommands = message.Dte2.Commands.Cast<Command>().ToList();
                var availableCommands = allCommands.Where(r => !string.IsNullOrEmpty(r.Name)).ToList();
                message.SearchControl.Dispatcher.Invoke(() => message.SearchControl.InitCommands(availableCommands));
            });

            Receive<KeyPressCommandSearchMessage>(message =>
            {
                var filtered = CommandSearchControl.FindCommandsByText(message.Text, message.SearchControl.AllCommandsListBoxItems);
                message.SearchControl.Dispatcher.Invoke(() => message.SearchControl.BindCommandsToControl(filtered));
            });
        }
    }

    public class KeyPressCommandSearchMessage
    {
        public CommandSearchControl SearchControl { get; set; }
        public string Text { get; set; }

        public KeyPressCommandSearchMessage(CommandSearchControl searchControl, string text)
        {
            SearchControl = searchControl;
            Text = text;
        }
    }
}