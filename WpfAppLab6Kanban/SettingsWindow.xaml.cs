using System.Windows;
using System.Windows.Media;
using WpfAppLab6Kanban.Data;

namespace WpfAppLab6Kanban
{
    public partial class SettingsWindow : Window
    {
        private readonly DatabaseService _db = new DatabaseService();

        public SettingsWindow()
        {
            InitializeComponent();
            LoadExistingSettings();
        }

        private void LoadExistingSettings()
        {
            // Load persistent states from DB.
            // Setting IsOn on a SettingsToggleRow triggers its
            // PropertyChangedCallback which keeps the inner CheckBox in sync.
            DarkModeRow.IsOn  = _db.GetSetting("DarkMode",    "0") == "1";
            BadgesRow.IsOn    = _db.GetSetting("ShowBadges",  "1") == "1";
        }

        // Handles the SettingsToggleRow.Toggled RoutedEvent — gives a live
        // Dark Mode preview as soon as the user flips the toggle.
        private void DarkModeRow_Toggled(object sender, RoutedEventArgs e)
            => ApplyTheme(DarkModeRow.IsOn);

        public void ApplyTheme(bool isDark)
        {
            if (isDark)
            {
                Application.Current.Resources["WindowBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202124"));
                Application.Current.Resources["CardBackgroundBrush"]   = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2E31"));
                Application.Current.Resources["TextBrush"]             = new SolidColorBrush(Colors.White);
                Application.Current.Resources["SecondaryTextBrush"]    = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9AA0A6"));
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#171717"));
            }
            else
            {
                Application.Current.Resources["WindowBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F2F5"));
                Application.Current.Resources["CardBackgroundBrush"]   = new SolidColorBrush(Colors.White);
                Application.Current.Resources["TextBrush"]             = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                Application.Current.Resources["SecondaryTextBrush"]    = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5F6368"));
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush(Colors.White);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _db.SaveSetting("DarkMode",   DarkModeRow.IsOn ? "1" : "0");
            _db.SaveSetting("ShowBadges", BadgesRow.IsOn   ? "1" : "0");
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
