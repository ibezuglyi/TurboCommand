

using Akka.Actor;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace TurboCommand.Package
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidsList.guidMenuAndCommandsPkg_string)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideToolWindow(typeof(CommandSearchWindow),  Orientation = ToolWindowOrientation.Top , 
        MultiInstances = false, Transient = true, 
        Width = 600, Height = 30, 
        PositionX = 300, PositionY = 300, 
        DocumentLikeTool = false)]
    [ComVisible(true)]
    public sealed class MenuCommandsPackage : Microsoft.VisualStudio.Shell.Package
    {
        private DteInitializer _dteInitializer;
        private DTE2 dte;
        private ActorSystem commandSystem;
        public MenuCommandsPackage()
        {
            commandSystem = ActorSystem.Create("CommandSystem");
        }

        protected override void Initialize()
        {
            base.Initialize();
            InitializeDTE();

            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                var commandId = new CommandID(GuidsList.guidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidMyCommand);
                var command = new OleMenuCommand(new EventHandler(ShowSearchCallback), commandId);
                mcs.AddCommand(command);
            }
        }

        private void ShowSearchCallback(object sender, EventArgs e)
        {
            ToolWindowPane window = this.FindToolWindow(typeof(CommandSearchWindow), 0, true);
            var mytoolwindow = (window as CommandSearchWindow);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("nie mogę!");
            }
            
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            mytoolwindow.InitActorSystem(commandSystem, dte);
        }

        private void InitializeDTE()
        {
            IVsShell shellService;

            this.dte = this.GetService(typeof(SDTE)) as DTE2;

            if (this.dte == null) 
            {
                shellService = this.GetService(typeof(SVsShell)) as IVsShell;
                this._dteInitializer = new DteInitializer(shellService, this.InitializeDTE);
            }
            else
            {
                this._dteInitializer = null;
            }
        }
    }
}
