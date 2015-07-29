using EnvDTE80;

namespace TurboCommand.Package
{
    public class CommandSearchMessage
    {
        public DTE2 Dte2 { get; set; }
        public CommandSearchControl SearchControl { get; set; }

        public CommandSearchMessage(DTE2 dte2, CommandSearchControl searchControl)
        {
            Dte2 = dte2;
            SearchControl = searchControl;
        }
    }
}