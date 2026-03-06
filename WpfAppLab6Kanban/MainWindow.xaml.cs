using System.Windows;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;
using WpfAppLab6Kanban.ViewModels;

namespace WpfAppLab6Kanban
{
    // ------------------------------------------------------------------
    //  MainWindow — the View in MVVM.
    //
    //  Topic 2 change: Now that the toolkit generates all commands and
    //  the ViewModel is fully self-contained, the View's only jobs are:
    //    1. Create the ViewModel and set DataContext
    //    2. Subscribe to ViewModel events that require opening a Window
    //       (the VM cannot open windows itself without importing WPF)
    //    3. Forward dialog results back to the ViewModel
    //
    //  Notice: no business logic, no database calls, no collections.
    // ------------------------------------------------------------------
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainViewModel(new DatabaseService());
            DataContext = _vm;

            // Subscribe to the ViewModel's dialog-request events.
            // The VM raises these; the View reacts by opening the window.
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

        // ── Thin UI-only handlers (cannot be replaced by commands) ────

        // Opens the hamburger ContextMenu — purely a UI concern
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
            => HamburgerButton.ContextMenu.IsOpen = true;

        // NOTE: ListBox double-click is now handled inside KanbanColumnControl.
        // When the user double-clicks a card, that control executes
        // ItemDoubleClickCommand (bound to EditTaskCommand on the ViewModel),
        // which raises RequestEditTask → OpenEditTaskDialog opens here.
    }
}