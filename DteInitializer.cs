using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace TurboCommand.Package
{
    internal class DteInitializer : IVsShellPropertyEvents
    {
        private IVsShell shellService;
        private uint cookie;
        private Action callback;

        internal DteInitializer(IVsShell shellService, Action callback)
        {
            this.shellService = shellService;
            this.callback = callback;

            int hr = this.shellService.AdviseShellPropertyChanges(this, out this.cookie);

            ErrorHandler.ThrowOnFailure(hr);
        }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            int hr;
            bool isZombie;

            if (propid == (int)__VSSPROPID.VSSPROPID_Zombie)
            {
                isZombie = (bool)var;

                if (!isZombie)
                {
                    hr = this.shellService.UnadviseShellPropertyChanges(this.cookie);

                    ErrorHandler.ThrowOnFailure(hr);

                    this.cookie = 0;

                    this.callback();
                }
            }
            return VSConstants.S_OK;
        }
    }
}