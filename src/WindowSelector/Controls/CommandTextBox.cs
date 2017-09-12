using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WindowSelector.Controls
{
    [TemplatePart(Name = "PART_selector", Type = typeof(ComboBox))]
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(Control))]
    public class CommandTextBox : TextBox
    {
        static CommandTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandTextBox), new FrameworkPropertyMetadata(typeof(CommandTextBox)));
            
        }

        public CommandTextBox()
        {
            Mappings = new CommandMappingCollection();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _comboBox = (ComboBox) Template.FindName("PART_selector", this);
            if (_comboBox != null)
            {
                _comboBox.PreviewKeyDown += ComboBox_PreviewKeyDown;
            }

            _content = (Control) Template.FindName("PART_ContentHost", this);

        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                Focus();
            }
        }

        private void OnActiveCommandChanged(CommandDescription oldCommand, CommandDescription newCommand)
        {
            UpdateCommandAreaVisibility();
        }

        private void UpdateCommandAreaVisibility()
        {
            CommandAreaVisibility = (_isCommandAreaAlwaysVisible ||  ActiveCommand != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnMappingsChanged(ObservableCollection<CommandDescription> oldValue, ObservableCollection<CommandDescription> newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= MappingsOnCollectionChanged;

            }

            if (newValue != null)
            {
                newValue.CollectionChanged += MappingsOnCollectionChanged;

                if (!newValue.Contains(ActiveCommand))
                {
                    ActiveCommand = Mappings.DefaultCommand;
                }
            }
        }

        private void MappingsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ActiveCommand == null)
            {
                ActiveCommand = Mappings.DefaultCommand;
            }
        }

        //#region public CommandMapping DefaultCommand
        //public CommandMapping DefaultCommand
        //{
        //    get { return (CommandMapping)GetValue(DefaultCommandProperty); }
        //    set { SetValue(DefaultCommandProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for DefaultCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DefaultCommandProperty =
        //    DependencyProperty.Register("DefaultCommand", typeof(CommandMapping), typeof(CommandTextBoxCtl), new FrameworkPropertyMetadata(OnDefaultCommandChanged));

        //private static void OnDefaultCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        //{
        //    var textBox = (CommandTextBoxCtl)dependencyObject;
        //    textBox.OnDefaultCommandChanged((CommandMapping) e.OldValue, (CommandMapping) e.NewValue);
        //}
        //#endregion

        #region public CommandMapping ActiveCommand
        public CommandDescription ActiveCommand
        {
            get { return (CommandDescription)GetValue(ActiveCommandProperty); }
            set { SetValue(ActiveCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveMapping.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveCommandProperty =
            DependencyProperty.Register("ActiveCommand", typeof(CommandDescription), typeof(CommandTextBox), new FrameworkPropertyMetadata(OnActiveCommandChanged));

        private static void OnActiveCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (CommandTextBox)dependencyObject;
            var oldCommand = (CommandDescription) e.OldValue;
            var newCommand = (CommandDescription) e.NewValue;
            textBox.OnActiveCommandChanged(oldCommand, newCommand);

        }

        #endregion

        #region public Visibility CommandAreaVisibility { get; private set; }

        public Visibility CommandAreaVisibility
        {
            get { return (Visibility)GetValue(CommandAreaVisibilityProperty); }
            set { SetValue(CommandAreaVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandAreaVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandAreaVisibilityProperty =
            DependencyProperty.Register("CommandAreaVisibility", typeof(Visibility), typeof(CommandTextBox), new PropertyMetadata(Visibility.Collapsed));


        #endregion

        #region public CommandMappingCollection Mappings
        public CommandMappingCollection Mappings
        {
            get { return (CommandMappingCollection)GetValue(MappingsProperty); }
            set { SetValue(MappingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Mappings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MappingsProperty =
            DependencyProperty.Register("Mappings", typeof(ObservableCollection<CommandDescription>), typeof(CommandTextBox), new FrameworkPropertyMetadata(OnMappingsChanged, MappingsCoerceValueCallback));
        private bool _isCommandAreaAlwaysVisible;
        private ComboBox _comboBox;
        private Control _content;

        private static object MappingsCoerceValueCallback(DependencyObject dependencyObject, object baseValue)
        {
            if (baseValue == null)
            {
                return new ObservableCollection<CommandDescription>();
            }
            return baseValue;
        }

        private static void OnMappingsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (CommandTextBox)dependencyObject;

            var oldValue = (ObservableCollection<CommandDescription>)e.OldValue;
            var newValue = (ObservableCollection<CommandDescription>)e.NewValue;

            textBox.OnMappingsChanged(oldValue, newValue);
        }
        #endregion

        public bool IsCommandAreaAlwaysVisible
        {
            get
            {
                return _isCommandAreaAlwaysVisible;
            }
            set
            {
                _isCommandAreaAlwaysVisible = value;
                UpdateCommandAreaVisibility();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (ActiveCommand == null || ActiveCommand == Mappings.DefaultCommand) return;

            if (e.Key == Key.Back && (Text.Length == 0 || CaretIndex == 0))
            {
                var textToAdd = ActiveCommand.Alias;
                ActiveCommand = Mappings.DefaultCommand;
                Text = textToAdd + Text;
                CaretIndex = textToAdd.Length;
                e.Handled = true;
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            SetActiveCommand();
            base.OnTextChanged(e);
        }

        private void SetActiveCommand()
        {
            var matched = Mappings.FirstOrDefault(m => Text.StartsWith(m.Alias + " "));
            if (matched != null)
            {
                ActiveCommand = matched;
                Text = Text.Substring(matched.Alias.Length + 1);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            SetActiveCommand();
        }

        public void Reset()
        {
            Text = String.Empty;
            ActiveCommand = Mappings?.DefaultCommand;
        }
    }
}
