using System.Windows;
using System.Windows.Media;

namespace WpfAppLab6Kanban
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DarkModeToggle.IsChecked = IsDarkModeActive();
        }

        private bool IsDarkModeActive()
        {
            var brush = Application.Current.Resources["WindowBackgroundBrush"] as SolidColorBrush;
            return brush?.Color == ((SolidColorBrush)new BrushConverter().ConvertFrom("#202124")).Color;
        }

        private void DarkModeToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isDark = DarkModeToggle.IsChecked ?? false;
            
            if (isDark)
            {
                Application.Current.Resources["WindowBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202124"));
                Application.Current.Resources["CardBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2E31"));
                Application.Current.Resources["TextBrush"] = new SolidColorBrush(Colors.White);
                Application.Current.Resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9AA0A6"));
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#171717"));
            }
            else
            {
                Application.Current.Resources["WindowBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F2F5"));
                Application.Current.Resources["CardBackgroundBrush"] = new SolidColorBrush(Colors.White);
                Application.Current.Resources["TextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                Application.Current.Resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5F6368"));
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush(Colors.White);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
