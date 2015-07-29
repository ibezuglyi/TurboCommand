using Akka.Actor;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace TurboCommand.Package
{
    [Guid("86c9d038-84a5-45e3-b78b-8766a46c3dee")]
    public class CommandSearchWindow : ToolWindowPane
    {
        private DTE2 dte;
        private DteInitializer dteInitializer;
        public ActorSystem CommandSystem { get; set; }
        public CommandSearchControl SearchControl { get; set; }

        public CommandSearchWindow() :
            base(null)
        {
            this.Caption = "Search for command";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            SearchControl = new CommandSearchControl();
            base.Content = SearchControl;
            SearchControl.ReadyToBeClosed += CloseWindow;
            SearchControl.OnRaiseCommand += RaiseCommand;
        }


        private void RaiseCommand(object sender, OnCommandEventArgs eventArgs)
        {
            try
            {
                this.dte.Commands.Raise(eventArgs.CommandGuid, eventArgs.CommandId, null, null);
            }
            catch (Exception e)
            {
                this.dte.ExecuteCommand(eventArgs.CommandFullName);
            }
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame = (IVsWindowFrame)this.Frame;
            windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
        }

       

        public void InitActorSystem(ActorSystem commandSystem, DTE2 dte2)
        {
            dte = dte2;
            CommandSystem = commandSystem;
            var commandSearcherActor = CommandSystem.ActorOf<CommandSearcher>();
            SearchControl.InitActorSystem(commandSearcherActor);
            
            commandSearcherActor.Tell(new CommandSearchMessage(dte2, SearchControl));
        }
    }
}
