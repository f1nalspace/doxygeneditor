using System.Windows.Forms;

namespace DoxygenEditor.Services
{
    class WinFormsMessageBoxService : IMessageBoxService
    {
        private readonly IWin32Window _owner;

        public WinFormsMessageBoxService(IWin32Window owner)
        {
            _owner = owner;
        }

        private MessageBoxButtons ConvertTo(MsgBoxButtons buttons)
        {
            MessageBoxButtons result = (MessageBoxButtons)(int)buttons;
            return (result);
        }
        private MessageBoxIcon ConvertTo(MsgBoxIcon icon)
        {
            MessageBoxIcon result = (MessageBoxIcon)(int)icon;
            return (result);
        }
        private MsgBoxResult ConvertTo(DialogResult dialogResult)
        {
            MsgBoxResult result = (MsgBoxResult)(int)dialogResult;
            return (result);
        }
        public MsgBoxResult Show(string text, string caption, MsgBoxButtons buttons, MsgBoxIcon icon)
        {
            MessageBoxButtons localButtons = ConvertTo(buttons);
            MessageBoxIcon localIcon = ConvertTo(icon);
            DialogResult r = MessageBox.Show(_owner, text, caption, localButtons, localIcon);
            MsgBoxResult result = ConvertTo(r);
            return (result);
        }
    }
}
