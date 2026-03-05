using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;
using WpfAppLab6Kanban.ViewModels;

namespace WpfAppLab6Kanban
{
    // Code-behind is now ONLY responsible for View concerns:
    //   - Creating and wiring the ViewModel
    //   - Handling UI interactions that cannot be expressed in XAML commands
    //     (e.g. MouseDoubleClick on a ListBox, opening the hamburger menu)
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            var db = new DatabaseService();
            _vm = new MainViewModel(db);
            _vm.ApplyStartupSettings();

            DataContext = _vm;
        }

        // The hamburger ContextMenu opens on left-click; this cannot be done in XAML alone.
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
            => HamburgerButton.ContextMenu.IsOpen = true;

        // Double-click on a task card opens the detail window via ViewModel command.
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem is KanbanTask task)
                _vm.OpenTaskCommand.Execute(task);
        }
    }
}