using System;
using System.Windows.Input;
using WindowSelector.Common.ViewModels;

namespace WindowSelector.Commands
{
    public class WhiteListItemCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter == null) return;
            var item = (WindowResult) parameter;
            if (!item.IsWhiteListed)
            {
                item.Whitelist();
            }
            else
            {
                item.UnWhitelist();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}
