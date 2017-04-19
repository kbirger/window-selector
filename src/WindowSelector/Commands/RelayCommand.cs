using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WindowSelector.Commands
{
    /// <summary>
    /// Implementation of ICommand which uses delegates for actions
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> canExecute;

        private readonly Action<object> execute;

        private EventHandler canExecuteEventhandler;

        /// <summary>
        /// Instantiates <see cref="RelayCommand"/> with an execute delegate 
        /// </summary>
        /// <param name="execute">Delegate representing action to perform when command is invoked</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Instantiates <see cref="RelayCommand"/> with an execute delegate and canExecute delegate
        /// </summary>
        /// <param name="execute">Delegate representing action to perform when command is invoked</param>
        /// <param name="canExecute">Delegate which is to return true when command can be executed</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Event raised when state of command changes in a way that affects whether it can be executed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                canExecuteEventhandler += value;
            }

            remove
            {
                canExecuteEventhandler -= value;
            }
        }

        /// <summary>
        /// Returns true if command can be executed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes command
        /// </summary>
        /// <param name="parameter"></param>
        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            execute?.Invoke(parameter);
        }

        /// <summary>
        /// Trigger command execution state changed event
        /// </summary>
        public void InvokeCanExecuteChanged()
        {
            if (this.canExecute != null)
            {
                this.canExecuteEventhandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
