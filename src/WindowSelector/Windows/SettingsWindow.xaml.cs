using System.ComponentModel;
using System.Windows;
using WindowSelector.ViewModels;

namespace WindowSelector.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private SettingsWindowViewModel _vm;

        public SettingsWindow(SettingsWindowViewModel viewModel)
        {
            InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _vm.Save();
            Hide();
        }
    }
}
