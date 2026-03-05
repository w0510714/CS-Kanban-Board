using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.ViewModels
{
<<<<<<< HEAD
<<<<<<< HEAD
    // ======================================================================
    //  MainViewModel — MVVM Toolkit edition
    // ======================================================================
    //
    //  BEFORE (manual boilerplate):
    //    • Implemented INotifyPropertyChanged by hand
    //    • Wrote private backing fields + full property bodies for every
    //      bindable property (get/set with OnPropertyChanged call)
    //    • Created RelayCommand instances manually in the constructor
    //    • Maintained a separate RelayCommand.cs + RelayCommand<T> class
    //
    //  AFTER (CommunityToolkit.Mvvm source generators):
    //    • Inherit from ObservableObject  → INotifyPropertyChanged handled
    //    • [ObservableProperty] on a private field               → full
    //      public property + notification auto-generated at compile time
    //    • [RelayCommand] on a private method                    → public
    //      IRelayCommand property auto-generated at compile time
    //    • [RelayCommand(CanExecute = nameof(...))] adds guards
    //    • RelayCommand.cs can be deleted — toolkit ships its own
    //
    //  The class MUST be declared `partial` so the source generator can
    //  add the generated code in a companion file.
    // ======================================================================
    public partial class MainViewModel : ObservableObject
    {
        // ──────────────────────────────────────────────────────────────────
        //  Infrastructure
        // ──────────────────────────────────────────────────────────────────
        private readonly DatabaseService _db;

        // ──────────────────────────────────────────────────────────────────
        //  Observable collections — bound to the three ListBox controls
        // ──────────────────────────────────────────────────────────────────
        public ObservableCollection<KanbanTask> TodoTasks       { get; } = new();
        public ObservableCollection<KanbanTask> InProgressTasks { get; } = new();
        public ObservableCollection<KanbanTask> DoneTasks       { get; } = new();

        // ──────────────────────────────────────────────────────────────────
        //  [ObservableProperty] — replaces manual backing field + property
        //
        //  The toolkit source generator turns each attributed private field
        //  into a full public property with INotifyPropertyChanged support.
        //
        //  Example — what the generator produces for _badgeVisibility:
        //
        //      public Visibility BadgeVisibility
        //      {
        //          get => _badgeVisibility;
        //          set => SetProperty(ref _badgeVisibility, value);
        //      }
        //
        //  We write 2 lines; the generator writes ~6 lines per property.
        // ──────────────────────────────────────────────────────────────────

        /// <summary>Controls priority-badge visibility; toggled by the Settings window.</summary>
        [ObservableProperty]
        private Visibility _badgeVisibility = Visibility.Visible;

        /// <summary>True when at least one active task exists; guards ArchiveAllCommand.</summary>
        [ObservableProperty]
        private bool _hasTasks;

        // ──────────────────────────────────────────────────────────────────
        //  Constructor
        // ──────────────────────────────────────────────────────────────────
        public MainViewModel(DatabaseService db)
        {
            _db = db;
            ApplyStartupSettings();
            LoadTasks();
        }

        // ──────────────────────────────────────────────────────────────────
        //  [RelayCommand] — replaces manual ICommand wiring
        //
        //  The toolkit source generator converts each [RelayCommand]-
        //  decorated private method into a public IRelayCommand property.
        //
        //  Naming convention (automatic):
        //      private void AddTask()   →  public IRelayCommand AddTaskCommand
        //      private void MoveLeft()  →  public IRelayCommand MoveLeftCommand
        //
        //  CanExecute guard (automatic enable/disable of bound buttons):
        //      [RelayCommand(CanExecute = nameof(CanArchiveAll))]
        //      → calls CanArchiveAll() before every execute attempt
        //
        //  No constructor wiring, no RelayCommand<T> class needed.
        // ──────────────────────────────────────────────────────────────────

        /// <summary>Raises an event so the View can open the Add-Task dialog.</summary>
        [RelayCommand]
        private void AddTask() => RequestAddTask?.Invoke();

        /// <summary>Raises an event so the View can open the Edit-Task dialog.</summary>
        [RelayCommand]
        private void EditTask(KanbanTask task) => RequestEditTask?.Invoke(task);

        /// <summary>Moves a card one column to the left.</summary>
        [RelayCommand]
        private void MoveLeft(KanbanTask task)
        {
            if (task is null) return;
=======
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
    // The ViewModel owns all state and commands for the main board.
    // [ObservableProperty] generates backing fields + INotifyPropertyChanged.
    // [RelayCommand] generates ICommand implementations — no boilerplate required.
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        // --- Observable Collections (the three Kanban columns) ---
        public ObservableCollection<KanbanTask> TodoTasks { get; } = new();
        public ObservableCollection<KanbanTask> InProgressTasks { get; } = new();
        public ObservableCollection<KanbanTask> DoneTasks { get; } = new();

        // Controls priority badge visibility; toggled from settings
        [ObservableProperty]
        private Visibility _badgeVisibility = Visibility.Visible;

        // Drives the "Archive All / End Sprint" menu item enabled state
        [ObservableProperty]
        private bool _hasTasks;

        public MainViewModel(DatabaseService db)
        {
            _db = db;
            LoadTasks();
        }

        // -------------------------------------------------------
        //  Commands — each [RelayCommand] method becomes a
        //  public IRelayCommand property automatically.
        // -------------------------------------------------------

        /// <summary>Opens the Add-Task dialog and persists the result.</summary>
        [RelayCommand]
        private void AddTask()
        {
            var win = new AddTaskWindow { Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true && win.NewTask != null)
            {
                _db.AddTask(win.NewTask);
                TodoTasks.Add(win.NewTask);
                RefreshHasTasks();
            }
        }

        /// <summary>Opens the detail/edit dialog for the selected task.</summary>
        [RelayCommand]
        private void OpenTask(KanbanTask task)
        {
            if (task == null) return;
            string originalColumn = task.Column;
            var win = new TaskDetailWindow(task) { Owner = Application.Current.MainWindow };

            if (win.ShowDialog() == true)
            {
                if (win.IsDeleted)
                {
                    _db.DeleteTask(task.Id);
                    GetCollection(originalColumn).Remove(task);
                }
                else
                {
                    _db.UpdateTask(task);
                    if (task.Column != originalColumn)
                    {
                        GetCollection(originalColumn).Remove(task);
                        GetCollection(task.Column).Add(task);
                    }
                    if (task.IsArchived)
                        GetCollection(task.Column).Remove(task);
                }
                RefreshHasTasks();
            }
        }

        /// <summary>Moves a task one column to the left.</summary>
        [RelayCommand]
        private void MoveLeft(KanbanTask task)
        {
            if (task == null) return;
<<<<<<< HEAD
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
            string newColumn = task.Column switch
            {
                "In Progress" => "To Do",
                "Done"        => "In Progress",
                _             => task.Column
            };
            MoveTask(task, newColumn);
        }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>Moves a card one column to the right.</summary>
        [RelayCommand]
        private void MoveRight(KanbanTask task)
        {
            if (task is null) return;
=======
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        /// <summary>Moves a task one column to the right.</summary>
        [RelayCommand]
        private void MoveRight(KanbanTask task)
        {
            if (task == null) return;
<<<<<<< HEAD
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
            string newColumn = task.Column switch
            {
                "To Do"       => "In Progress",
                "In Progress" => "Done",
                _             => task.Column
            };
            MoveTask(task, newColumn);
        }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>Permanently deletes a task after a confirmation prompt.</summary>
        [RelayCommand]
        private void DeleteTask(KanbanTask task)
        {
            if (task is null) return;
            if (MessageBox.Show($"Permanently delete '{task.Title}'?", "Confirm Delete",
=======
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        /// <summary>Permanently deletes a task after confirmation.</summary>
        [RelayCommand]
        private void DeleteTask(KanbanTask task)
        {
            if (task == null) return;
            if (MessageBox.Show($"Permanently delete '{task.Title}'?", "Confirm",
<<<<<<< HEAD
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _db.DeleteTask(task.Id);
                GetCollection(task.Column).Remove(task);
                RefreshHasTasks();
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>
        /// Archives every active task (ends the sprint).
        /// CanExecute = HasTasks — when HasTasks is false the button is
        /// automatically disabled by the toolkit; no manual wiring needed.
        /// </summary>
        [RelayCommand(CanExecute = nameof(HasTasks))]
        private void ArchiveAll()
        {
            if (MessageBox.Show("Archive all tasks and clear the board?", "End Sprint",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
=======
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        /// <summary>Archives every active task (ends the sprint).</summary>
        [RelayCommand(CanExecute = nameof(HasTasks))]
        private void ArchiveAll()
        {
            if (MessageBox.Show("Archive all tasks and clear the board?", "Confirm",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
<<<<<<< HEAD
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
            {
                _db.ArchiveAllTasks();
                LoadTasks();
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>Raises an event so the View can open the Archives dialog.</summary>
        [RelayCommand]
        private void ViewArchives() => RequestViewArchives?.Invoke();

        /// <summary>Raises an event so the View can open the Settings dialog.</summary>
        [RelayCommand]
        private void OpenSettings() => RequestOpenSettings?.Invoke();

        /// <summary>Raises an event so the View can open the Help dialog.</summary>
        [RelayCommand]
        private void OpenHelp() => RequestOpenHelp?.Invoke();

        // ──────────────────────────────────────────────────────────────────
        //  Event delegation
        //  The ViewModel stays decoupled from WPF Window types by raising
        //  plain .NET events; the View subscribes and opens the dialogs.
        // ──────────────────────────────────────────────────────────────────
        public event Action?             RequestAddTask;
        public event Action<KanbanTask>? RequestEditTask;
        public event Action?             RequestViewArchives;
        public event Action?             RequestOpenSettings;
        public event Action?             RequestOpenHelp;

        // ──────────────────────────────────────────────────────────────────
        //  Public methods called by the View after dialog results
        // ──────────────────────────────────────────────────────────────────

        /// <summary>Persists and adds a newly created task to the board.</summary>
        public void CommitNewTask(KanbanTask task)
        {
            _db.AddTask(task);
            TodoTasks.Add(task);
            RefreshHasTasks();
        }

        /// <summary>Persists the result of an edit/delete operation.</summary>
        public void CommitEditedTask(KanbanTask task, string originalColumn, bool isDeleted)
        {
            if (isDeleted)
            {
                _db.DeleteTask(task.Id);
                GetCollection(originalColumn).Remove(task);
            }
            else
            {
                _db.UpdateTask(task);
                if (task.Column != originalColumn)
                {
                    GetCollection(originalColumn).Remove(task);
                    GetCollection(task.Column).Add(task);
                }
                if (task.IsArchived)
                    GetCollection(task.Column).Remove(task);
            }
            RefreshHasTasks();
        }

        /// <summary>Reloads all active tasks from the database.</summary>
=======
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        /// <summary>Opens the archive viewer; reloads active tasks on return.</summary>
        [RelayCommand]
        private void ViewArchives()
        {
            new ArchiveWindow { Owner = Application.Current.MainWindow }.ShowDialog();
            LoadTasks();
        }

        /// <summary>Opens the settings dialog and reapplies any changed settings.</summary>
        [RelayCommand]
        private void OpenSettings()
        {
            if (new SettingsWindow { Owner = Application.Current.MainWindow }.ShowDialog() == true)
                ApplyStartupSettings();
        }

        /// <summary>Opens the Help window.</summary>
        [RelayCommand]
        private void OpenHelp()
        {
            new HelpWindow { Owner = Application.Current.MainWindow }.ShowDialog();
        }

        // -------------------------------------------------------
        //  Private helpers
        // -------------------------------------------------------

        /// <summary>Loads (or reloads) all active tasks from the database.</summary>
<<<<<<< HEAD
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        public void LoadTasks()
        {
            TodoTasks.Clear();
            InProgressTasks.Clear();
            DoneTasks.Clear();

            foreach (var task in _db.GetAllTasks())
            {
                switch (task.Column)
                {
                    case "To Do":       TodoTasks.Add(task);       break;
                    case "In Progress": InProgressTasks.Add(task); break;
                    case "Done":        DoneTasks.Add(task);       break;
                }
            }
            RefreshHasTasks();
        }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>Reads persisted settings and applies them to the app.</summary>
=======
        /// <summary>Reads persisted settings and applies them to the application.</summary>
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
        /// <summary>Reads persisted settings and applies them to the application.</summary>
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        public void ApplyStartupSettings()
        {
            bool isDark     = _db.GetSetting("DarkMode",   "0") == "1";
            bool showBadges = _db.GetSetting("ShowBadges", "1") == "1";

            new SettingsWindow().ApplyTheme(isDark);
            BadgeVisibility = showBadges ? Visibility.Visible : Visibility.Collapsed;
        }

<<<<<<< HEAD
<<<<<<< HEAD
        // ──────────────────────────────────────────────────────────────────
        //  Private helpers
        // ──────────────────────────────────────────────────────────────────

=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        private void MoveTask(KanbanTask task, string newColumn)
        {
            if (newColumn == task.Column) return;
            string oldColumn = task.Column;
            task.Column = newColumn;
            _db.UpdateTask(task);
            GetCollection(oldColumn).Remove(task);
            GetCollection(newColumn).Add(task);
        }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>
        /// Updates HasTasks and tells the toolkit to re-evaluate
        /// ArchiveAllCommand.CanExecute — this is how the "End Sprint"
        /// menu item auto-enables and auto-disables.
        /// </summary>
        private void RefreshHasTasks()
        {
            HasTasks = TodoTasks.Count > 0 || InProgressTasks.Count > 0 || DoneTasks.Count > 0;
=======
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        private void RefreshHasTasks()
        {
            HasTasks = TodoTasks.Count > 0 || InProgressTasks.Count > 0 || DoneTasks.Count > 0;
            // Re-evaluate the CanExecute of ArchiveAllCommand
<<<<<<< HEAD
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
            ArchiveAllCommand.NotifyCanExecuteChanged();
        }

        private ObservableCollection<KanbanTask> GetCollection(string column) => column switch
        {
            "To Do"       => TodoTasks,
            "In Progress" => InProgressTasks,
            "Done"        => DoneTasks,
<<<<<<< HEAD
<<<<<<< HEAD
            _             => throw new ArgumentException($"Invalid column '{column}'", nameof(column))
=======
            _             => throw new ArgumentException("Invalid column name", nameof(column))
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
            _             => throw new ArgumentException("Invalid column name", nameof(column))
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        };
    }
}
