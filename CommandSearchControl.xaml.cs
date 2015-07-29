using System.Text.RegularExpressions;
using Akka.Actor;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TurboCommand.Package.Items;

namespace TurboCommand.Package
{
    public partial class CommandSearchControl : UserControl
    {
        public List<Command> AvailableCommands { get; set; }
        public volatile List<CommandListBoxItem> AllCommandsListBoxItems;
        public event EventHandler ReadyToBeClosed;
        public event CommandEventHandler OnRaiseCommand;
        public IActorRef CommandSearcherActor { get; set; }

        public CommandSearchControl()
        {
            InitializeComponent();
            commandSearch.KeyDown += SearchEnterPressed;
            commandSearch.PreviewKeyUp += ArrowKeysPressed;
            AllCommandsListBoxItems = new List<CommandListBoxItem>();
            AvailableCommands = new List<Command>();
        }

        private void ArrowKeysPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                if (commandsList.SelectedIndex < commandsList.Items.Count)
                    commandsList.SelectedIndex++;
            }

            if (e.Key == Key.Up)
            {
                e.Handled = true;
                if (commandsList.SelectedIndex > 0)
                    commandsList.SelectedIndex--;
            }
        }

        public void InitCommands(List<Command> availableCommands)
        {
            this.AvailableCommands = availableCommands;
            this.AllCommandsListBoxItems = availableCommands.Select(r => MapToCommandListBoxItem(r))
                .OrderByDescending(r => r.IsEnabled)
                .ThenBy(r => r.CommandName)
                .ToList();
            FilterAndBind();
        }

        private void FilterAndBind()
        {
            var matchedCommands = FindCommandsByText(commandSearch.Text, AllCommandsListBoxItems);
            BindCommandsToControl(matchedCommands);
        }

        public void BindCommandsToControl(List<CommandListBoxItem> availableCommands)
        {
            ClearCommandList();

            foreach (var listBoxItem in availableCommands.Take(10))
            {
                this.commandsList.Items.Add(listBoxItem);
            }
        }

        private static CommandListBoxItem MapToCommandListBoxItem(Command r)
        {
            var namespaces = r.Name.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            return new CommandListBoxItem()
            {
                CommandFullName = r.Name,
                CommandName = namespaces.Last(),
                IsAvailable = r.IsAvailable,
                Module = string.Join(".", namespaces.Take(namespaces.Length - 1)),
                CommandGuid = r.Guid,
                CommandId = r.ID,
                IsEnabled = r.IsAvailable
            };
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(commandSearch.Text))
            {
                ClearCommandList();
            }
            else
            {
                CommandSearcherActor.Tell(new KeyPressCommandSearchMessage(this, commandSearch.Text));
                //FilterAndBind();
                
            }
        }

        public static List<CommandListBoxItem> FindCommandsByText(string commandtoken, List<CommandListBoxItem> allCommandsListBoxItems)
        {
            var newCommandList = new List<CommandListBoxItem>(allCommandsListBoxItems);
            var matchedCommands = newCommandList
                   .Where(r => Regex.Match(r.CommandName, Pattern(commandtoken), RegexOptions.IgnoreCase | RegexOptions.Compiled).Success)
                   .OrderByDescending(r => r.IsEnabled)
                   .ThenBy(r => r.CommandName)
                   .ToList();
            return matchedCommands;
        }
        private static string Pattern(string src)
        {
            return ".*" + String.Join(".*", src.ToCharArray());
        }

        private void ClearCommandList()
        {
            this.commandsList.Items.Clear();
        }

        private void SearchEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                RunSelectedCommand();
            }
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void RunSelectedCommand()
        {
            var selectedItem = commandsList.Items[commandsList.SelectedIndex];
            var listBoxItem = selectedItem as CommandListBoxItem;
            var selectedCommand = AllCommandsListBoxItems.Single(r => r.CommandName == listBoxItem.CommandName && r.Module == listBoxItem.Module);
            if (OnRaiseCommand != null && selectedCommand.IsAvailable)
            {
                OnRaiseCommand(this, new OnCommandEventArgs(selectedCommand.CommandGuid, selectedCommand.CommandId, selectedCommand.CommandFullName));
            }

            Close();
        }

        public void TbSearch_OnGotKeyboardFocus(object sender, RoutedEventArgs e)
        {

        }

        public void TbSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Close()
        {
            //cleaning up
            commandSearch.Text = String.Empty;
            commandsList.Items.Clear();

            //hide the window
            if (ReadyToBeClosed != null)
                ReadyToBeClosed(this, null);
        }


        public void InitActorSystem(IActorRef commandSearcherActor)
        {
            CommandSearcherActor = commandSearcherActor;
        }

        
    }
}
