using System.Windows.Forms;
using System.Windows.Input;

namespace DoxygenEditor.MVVM
{
    public static class CommandUtils
    {
        public static void BindClickCommand(ToolStripMenuItem menuItem, ICommand command)
        {
            menuItem.Click += (s, e) =>
            {
                command.Execute(null);
            };
            command.CanExecuteChanged += (s, e) =>
            {
                menuItem.Enabled = command.CanExecute(null);
            };
        }       
        public static void BindClickCommand(ToolStripButton toolButton, ICommand command)
        {
            toolButton.Click += (s, e) =>
            {
                command.Execute(null);
            };
            command.CanExecuteChanged += (s, e) =>
            {
                toolButton.Enabled = command.CanExecute(null);
            };
        }
        public static void BindCheckCommand(ToolStripMenuItem menuItem, ICommand command)
        {
            menuItem.Click += (s, e) =>
            {
                if (!menuItem.Checked)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
                command.Execute(menuItem.Checked);
            };
            command.CanExecuteChanged += (s, e) =>
            {
                menuItem.Enabled = command.CanExecute(menuItem.Checked);
            };
        }
    }
}
