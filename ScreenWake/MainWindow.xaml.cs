using System.Windows;
using System.Windows.Controls;

namespace ScreenWake
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        PowerScheme _scheme = new PowerScheme();

        public MainWindow()
        {
            InitializeComponent();
            InitializeDataContext();
        }

        public void InitializeDataContext()
        {
            DataContext = _scheme.UiBinding;
            _scheme.RefreshUi();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "Refresh":
                    _scheme.RefreshUi();
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _scheme.SaveSettings();
        }
    }
}
