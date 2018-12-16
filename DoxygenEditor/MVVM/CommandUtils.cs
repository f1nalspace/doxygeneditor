using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace DoxygenEditor.MVVM
{
    public static class CommandUtils
    {
        class CommandHistoryItem
        {
            public object Control { get; set; }
            public ICommand Command { get; set; }
            public EventHandler ClickHandler { get; set; }
            public EventHandler CanExecuteHandler { get; set; }
        }

        private static readonly Dictionary<object, CommandHistoryItem> _events = new Dictionary<object, CommandHistoryItem>();

        private static void UnbindCommand(ToolStripMenuItem menuItem)
        {
            if (_events.ContainsKey(menuItem))
            {
                var pair = _events[menuItem];
                menuItem.Click -= pair.ClickHandler;
                pair.Command.CanExecuteChanged -= pair.CanExecuteHandler;
                _events.Remove(menuItem);
            }
        }
        private static void UnbindCommand(ToolStripButton button)
        {
            if (_events.ContainsKey(button))
            {
                var pair = _events[button];
                button.Click -= pair.ClickHandler;
                pair.Command.CanExecuteChanged -= pair.CanExecuteHandler;
                _events.Remove(button);
            }
        }
        public static void BindClickCommand(ToolStripMenuItem menuItem, ICommand command)
        {
            UnbindCommand(menuItem);
            var historyItem = new CommandHistoryItem()
            {
                Command = command,
                Control = menuItem,
                ClickHandler = (s, e) =>
                {
                    command.Execute(null);
                },
                CanExecuteHandler = (s, e) =>
                {
                    menuItem.Enabled = command.CanExecute(null);
                }
            };
            menuItem.Click += historyItem.ClickHandler;
            command.CanExecuteChanged += historyItem.CanExecuteHandler;
            _events.Add(menuItem, historyItem);
        }
        public static void BindClickCommand(ToolStripButton toolButton, ICommand command)
        {
            UnbindCommand(toolButton);
            var historyItem = new CommandHistoryItem()
            {
                Command = command,
                Control = toolButton,
                ClickHandler = (s, e) =>
                {
                    command.Execute(null);
                },
                CanExecuteHandler = (s, e) =>
                {
                    toolButton.Enabled = command.CanExecute(null);
                }
            };
            toolButton.Click += historyItem.ClickHandler;
            command.CanExecuteChanged += historyItem.CanExecuteHandler;
            _events.Add(toolButton, historyItem);
        }
        public static void BindCheckCommand(ToolStripMenuItem menuItem, ICommand command)
        {
            UnbindCommand(menuItem);
            var historyItem = new CommandHistoryItem()
            {
                Command = command,
                Control = menuItem,
                ClickHandler = (s, e) =>
                {
                    if (!menuItem.Checked)
                        menuItem.Checked = true;
                    else
                        menuItem.Checked = false;
                    command.Execute(menuItem.Checked);
                },
                CanExecuteHandler = (s, e) =>
                {
                    menuItem.Enabled = command.CanExecute(menuItem.Checked);
                }
            };
            menuItem.Click += historyItem.ClickHandler;
            command.CanExecuteChanged += historyItem.CanExecuteHandler;
            _events.Add(menuItem, historyItem);
        }
    }
}
