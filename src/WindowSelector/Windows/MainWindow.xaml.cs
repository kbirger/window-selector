using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.ViewModels;
using WindowSelector.ViewModels;

namespace WindowSelector.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfigurationProvider _configurationProvider;
        //private readonly IWindowWatcher _windowWatcher;
        private readonly SettingsWindow _settingsWindow;
        private bool _stayOpen;
        private readonly ThumbnailWindow _thumbWindow;
        private bool _storedStayOpenState;

        public MainWindow(ThumbnailWindow thumbnailWindow, SettingsWindow settingsWindow, IConfigurationProvider configurationProvider, MainWindowViewModel viewModel)
        {
            InitializeComponent();
            _thumbWindow = thumbnailWindow;
            
            _settingsWindow = settingsWindow;
            //_thumbWindow.DataContext = FoundWindows;
            _configurationProvider = configurationProvider;
            //_windowWatcher = windowWatcher;
            var configuration = _configurationProvider.GetConfiguration();
            _configurationProvider.AddConfigurationUpdatedHandler(OnConfigurationUpdated);
            SetHotkeys(configuration);

            WindowStyle = WindowStyle.None;
            CenterWindowOnScreen();
            ViewModel = viewModel;
            //ViewModel.CollectionChanged += Results_CollectionChanged;
            ViewModel.WindowClosing += ViewModelOnWindowClosing;
            ViewModel.WindowClosed += ViewModelOnWindowClosed;
            ViewModel.WindowSelected += ViewModelOnWindowSelected;
            DataContext = ViewModel;
            _thumbWindow.InputBindings.Add(new MouseBinding()
            {
                MouseAction = MouseAction.LeftClick,
                Command = ViewModel.ItemSelectCommand

            });
            //FoundWindows.Items.SortDescriptions.Add(new SortDescription("Priority", ListSortDirection.Descending));
            //FoundWindows.Items.SortDescriptions.Add(new SortDescription("DisplayText", ListSortDirection.Descending));
            //var bindings = Resources["InputBindings"] as ICollection;
            //InputBindings.AddRange(bindings);
            //Input.InputBindings.AddRange(bindings);
            //FoundWindows.InputBindings.AddRange(bindings);

            //_windowWatcher.Start();
        }

        private void OnConfigurationUpdated(object sender, ConfigurationProvider.ConfigurationUpdateEventArgs args)
        {
            if (args.ChangedProperties.Any(x => x.Contains("Hotkeys")))
            {
                var configuration = _configurationProvider.GetConfiguration();
                SetHotkeys(configuration);
            }
        }

        private void ViewModelOnWindowSelected(object sender, MainWindowViewModel.WindowEventArgs windowEventArgs)
        {
            DoHide();
        }

        private void ViewModelOnWindowClosed(object sender, MainWindowViewModel.WindowEventArgs windowEventArgs)
        {
            Topmost = true;
            _thumbWindow.Topmost = true;
            _stayOpen = _storedStayOpenState;
            Focus();
            
        }

        private void ViewModelOnWindowClosing(object sender, MainWindowViewModel.WindowEventArgs windowEventArgs)
        {
            _storedStayOpenState = _stayOpen;
            _stayOpen = true;

            Topmost = false;
            _thumbWindow.Topmost = false;
        }

        public MainWindowViewModel ViewModel { get; private set; }

        private void CenterThumbnail()
        {
            var thumbLeft = Left + Width + 5;
            if (thumbLeft + _thumbWindow.Width > SystemParameters.PrimaryScreenWidth)
            {
                thumbLeft = SystemParameters.PrimaryScreenWidth - _thumbWindow.Width;
            }

            _thumbWindow.Left = thumbLeft;
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = ActualWidth;
            double windowHeight = ActualHeight;
            Top = 0;
            Left = 0;
            Height = SystemParameters.MaximizedPrimaryScreenHeight - 20;
        }
        private void CenterWindowOnScreen2()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = ActualWidth;
            double windowHeight = ActualHeight;
            Left = screenWidth/2 - windowWidth/2;
            Top = screenHeight/2 - windowHeight/2;
        }

        private void DoHide()
        {
            Hide();
            ViewModel.Reset();
            Input.Reset();
            ViewModel.IsHighlighting = false;
            _thumbWindow.Hide();
            _stayOpen = false;
            GC.Collect();
        }

        private void FoundWindows_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set new thumbnail when selection changes
            _thumbWindow.DataContext = FoundWindows.SelectedItem;
            FoundWindows.ScrollIntoView(FoundWindows.SelectedItem);
            // Re-center thumb window when a new thumbnail gets set
            CenterThumbnail();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.InRange(Key.A, Key.Z) || e.Key.InRange(Key.D0, Key.D9))
            {
                Input.Focus();
            }
        }

        public void Input_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear results if input is empty
            if (Input.Text.Length == 0)
            {
                //ViewModel.Reset();
                ViewModel.Default();

                FoundWindows.SelectedIndex = -1;
                return;
            }

            string search =  Input.Text;
            ViewModel.Search(search, new[] {Input.ActiveCommand?.Alias});
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            if (!_stayOpen)
            {
                DoHide();
            }
            //_centerTimer.Stop();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _settingsWindow.ShowDialog();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _thumbWindow.Show();
            //_centerTimer.Start();
            Input.Focus();
            CenterWindowOnScreen();

        }

        private void OnActivateHotkey(object sender, HotkeyEventArgs e)
        {
            if (_settingsWindow.IsActive)
            {
                e.Handled = false;
                return;
            }
            _stayOpen = false;
            ShowWindow();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DoClose();
        }

        private void DoClose()
        {
            _thumbWindow.Close();
            TaskbarIcon.Dispose();
            //_windowWatcher.Stop();
            Environment.Exit(0);
        }

        private void OnDebugHotkey(object sender, HotkeyEventArgs e)
        {
            if (_settingsWindow.IsActive)
            {
                e.Handled = false;
                return;
            }
            _stayOpen = true;
            ShowWindow();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            // When we resize, recenter thumbnail
            CenterThumbnail();
        }

        private void SetHotkeys(ConfigurationRoot configuration)
        {
            HotkeyManager.Current.AddOrReplace("Activate", configuration.Hotkeys["activate_key"].Key, configuration.Hotkeys["activate_key"].Modifiers, OnActivateHotkey);
            HotkeyManager.Current.AddOrReplace("Debug", configuration.Hotkeys["debug_key"].Key, configuration.Hotkeys["debug_key"].Modifiers, OnDebugHotkey);
        }

        private void ShowWindow()
        {
            if (Input.Text.Length > 0)
            {
                Input.Reset();
            }
            else
            {
                ViewModel.Default();
            }
            //ViewModel.Reset();
            Activate();
            Show();
            Input.Focus();
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            DoClose();
        }

        private void TaskbarIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            _stayOpen = false;
            ShowWindow();
        }

        private void Close_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            DoHide();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key.In(Key.LeftCtrl, Key.RightCtrl))
            {
                ViewModel.IsHighlighting = true;
            }
            else if(e.Key.InRange(Key.D0, Key.D9) && 
                (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl) ))
            {
                int index = ((int) e.Key - (int) Key.D0) - 1;
                if (index == -1)
                {
                    index = 10;
                }
                if (index >= this.FoundWindows.Items.Count) return;
                ViewModel.SelectedItem = this.FoundWindows.Items[index] as WindowResult;
                ViewModel.ItemSelectCommand.Execute(false);
                ViewModel.IsHighlighting = false;
            }

        }

        private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.In(Key.LeftCtrl, Key.RightCtrl))
            {
                ViewModel.IsHighlighting = false;
            }
        }
    }
}
