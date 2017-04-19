using System;
using System.Windows.Input;
using WindowSelector.Common.ViewModels;

namespace WindowSelector.Commands
{
    public class BlackListItemCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter == null) return;
            var item = (WindowResult)parameter;
            if (!item.IsBlackListed)
            {
                item.Blacklist();
            }
            else
            {
                item.UnBlacklist();
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
