using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;
using WpfAppLab6Kanban.ViewModels;

namespace WpfAppLab6Kanban
{
    // ------------------------------------------------------------------
    // MainWindow — the View in MVVM.
    //
    // Responsibilities of the View:
    //   ✔ Create the ViewModel and set it as DataContext
    //   ✔ Subscribe to ViewModel events that require opening child windows
    //   ✔ Forward dialog results back to the ViewModel
    //   ✗ NO business logic
    //   ✗ NO database calls
    //   ✗ NO direct manipulation of collections
    // ------------------------------------------------------------------
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            // Create the ViewModel — pass in the shared database service
            _vm = new MainViewModel(new DatabaseService());

            // Set as DataContext so all {Binding ...} expressions in XAML resolve here
            DataContext = _vm;

            // Subscribe to ViewModel events that require a real Window to open
            _vm.RequestAddTask      += OpenAddTaskDialog;
            _vm.RequestEditTask     += OpenEditTaskDialog;
            _vm.RequestViewArchives += OpenArchivesDialog;
            _vm.RequestOpenSettings += OpenSettingsDialog;
            _vm.RequestOpenHelp     += OpenHelpDialog;
        }

        // ──────────────────────────────────────────────────────────────
        // Dialog launchers  (View-only code — UI responsibility)
        // ──────────────────────────────────────────────────────────────

        private void OpenAddTaskDialog()
        {
            var dlg = new AddTaskWindow { Owner = this };
            if (dlg.ShowDialog() == true && dlg.NewTask != null)
                _vm.CommitNewTask(dlg.NewTask);
        }

        private void OpenEditTaskDialog(KanbanTask task)
        {
            string originalColumn = task.Column;
            var dlg = new TaskDetailWindow(task) { Owner = this };
            if (dlg.ShowDialog() == true)
                _vm.CommitEditedTask(task, originalColumn, dlg.IsDeleted);
        }

        private void OpenArchivesDialog()
        {
            new ArchiveWindow { Owner = this }.ShowDialog();
            _vm.LoadTasks();   // refresh board after archive changes
        }

        private void OpenSettingsDialog()
        {
            var dlg = new SettingsWindow { Owner = this };
            if (dlg.ShowDialog() == true)
                _vm.ApplyStartupSettings();
        }

        private void OpenHelpDialog() => new HelpWindow { Owner = this }.ShowDialog();

        // ──────────────────────────────────────────────────────────────
        // Remaining thin event handlers
        // (These cannot be replaced by commands because they involve
        //  UI-specific context like the hamburger ContextMenu, or the
        //  double-click ListBox selection pattern.)
        // ──────────────────────────────────────────────────────────────

        // Opens the hamburger ContextMenu on button click
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
            => HamburgerButton.ContextMenu.IsOpen = true;

        // Menu items — delegate immediately to ViewModel methods
        private void ViewArchives_Click(object sender, RoutedEventArgs e) => _vm.OpenArchives();
        private void Settings_Click(object sender, RoutedEventArgs e)     => _vm.OpenSettings();
        private void Help_Click(object sender, RoutedEventArgs e)         => _vm.OpenHelp();

        // Double-click on a task card opens the detail/edit dialog
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem is KanbanTask task)
                _vm.RaiseEditTask(task);
        }
    }
}