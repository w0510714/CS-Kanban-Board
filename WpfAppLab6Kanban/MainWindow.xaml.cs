using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;
using WpfAppLab6Kanban.ViewModels;

namespace WpfAppLab6Kanban
{
    // Code-behind is only responsible for View concerns:
    //   - Creating and wiring the ViewModel
    //   - Handling UI interactions that cannot be expressed in XAML commands
    //     (e.g. opening the hamburger menu)
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainViewModel(new DatabaseService());
            DataContext = _vm;

            // Subscribe to ViewModel events — VM raises these; View opens the dialogs.
            _vm.RequestAddTask      += OpenAddTaskDialog;
            _vm.RequestEditTask     += OpenEditTaskDialog;
            _vm.RequestViewArchives += OpenArchivesDialog;
            _vm.RequestOpenSettings += OpenSettingsDialog;
            _vm.RequestOpenHelp     += OpenHelpDialog;
        }

        // ── Dialog launchers ──────────────────────────────────────────

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
            _vm.LoadTasks();
        }

        private void OpenSettingsDialog()
        {
            var dlg = new SettingsWindow { Owner = this };
            if (dlg.ShowDialog() == true)
                _vm.ApplyStartupSettings();
        }

        private void OpenHelpDialog() => new HelpWindow { Owner = this }.ShowDialog();

        // ── Thin UI-only handlers ─────────────────────────────────────

        // Opens the hamburger ContextMenu on left-click (cannot be done in XAML alone).
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
            => HamburgerButton.ContextMenu.IsOpen = true;
    }
}