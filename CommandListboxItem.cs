using System.Windows.Controls;

namespace TurboCommand.Package.Items
{
    public class CommandListBoxItem : ListBoxItem
    {
        public string CommandFullName { get; set; }
        public string CommandName { get; set; }
        public string Module { get; set; }
        public bool IsAvailable { get; set; }
        public string CommandGuid { get; set; }
        public int CommandId { get; set; }
    }
}
