using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace WindowSelector.Controls
{
    public class CommandMappingCollection : ObservableCollection<CommandDescription>
    {
        private CommandDescription _defaultCommand;
        public  CommandDescription DefaultCommand
        {
            get { return _defaultCommand; }
            private set
            {
                if (value != null && _defaultCommand != null)
                {
                    throw new InvalidOperationException("Can only have one default command.");
                }
                _defaultCommand = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DefaultCommand"));
            }
        }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnAdd(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var newItems = e.NewItems;
                    OnRemove(newItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (DefaultCommand != null && e.OldItems.Contains(DefaultCommand))
                    {
                        DefaultCommand = null;
                    }
                    OnAdd(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    DefaultCommand = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            base.OnCollectionChanged(e);
        }

        private void OnRemove(IList items)
        {
            if (DefaultCommand != null && items.Contains(DefaultCommand))
            {
                DefaultCommand = null;
            }
        }

        private void OnAdd(IList items )
        {
            try
            {
                var newDefault = items.Cast<CommandDescription>().SingleOrDefault(m => m.IsDefault);
                if (newDefault != null)
                {
                    DefaultCommand = newDefault;
                }
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("CommandMappingCollection can have only one DefaultCommand");
            }
        }
    }
}
