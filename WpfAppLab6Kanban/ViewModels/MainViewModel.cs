using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.ViewModels
{
    // The ViewModel owns all state and commands for the main board.
    // [ObservableProperty] generates backing fields + INotifyPropertyChanged.
    // [RelayCommand] generates ICommand implementations — no boilerplate required.
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        // --- Observable Collections (the three Kanban columns) ---
        public ObservableCollection<KanbanTask> TodoTasks       { get; } = new();
        public ObservableCollection<KanbanTask> InProgressTasks { get; } = new();
        public ObservableCollection<KanbanTask> DoneTasks       { get; } = new();

        // Controls priority badge visibility; toggled from settings
        [ObservableProperty]
        private Visibility _badgeVisibility = Visibility.Visible;

        // Drives the "Archive All / End Sprint" menu item enabled state
        [ObservableProperty]
        private bool _hasTasks;

        public MainViewModel(DatabaseService db)
        {
            _db = db;
            ApplyStartupSettings();
            LoadTasks();
        }

        // -------------------------------------------------------
        //  Commands — each [RelayCommand] method becomes a
        //  public IRelayCommand property automatically.
        // -------------------------------------------------------

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
            string newColumn = task.Column switch
            {
                "In Progress" => "To Do",
                "Done"        => "In Progress",
                _             => task.Column
            };
            MoveTask(task, newColumn);
        }

        /// <summary>Moves a card one column to the right.</summary>
        [RelayCommand]
        private void MoveRight(KanbanTask task)
        {
            if (task is null) return;
            string newColumn = task.Column switch
            {
                "To Do"       => "In Progress",
                "In Progress" => "Done",
                _             => task.Column
            };
            MoveTask(task, newColumn);
        }

        /// <summary>Permanently deletes a task after a confirmation prompt.</summary>
        [RelayCommand]
        private void DeleteTask(KanbanTask task)
        {
            if (task is null) return;
            if (MessageBox.Show($"Permanently delete '{task.Title}'?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _db.DeleteTask(task.Id);
                GetCollection(task.Column).Remove(task);
                RefreshHasTasks();
            }
        }

        /// <summary>
        /// Archives every active task (ends the sprint).
        /// CanExecute = HasTasks — button auto-enables/disables via toolkit.
        /// </summary>
        [RelayCommand(CanExecute = nameof(HasTasks))]
        private void ArchiveAll()
        {
            if (MessageBox.Show("Archive all tasks and clear the board?", "End Sprint",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.ArchiveAllTasks();
                LoadTasks();
            }
        }

        /// <summary>Raises an event so the View can open the Archives dialog.</summary>
        [RelayCommand]
        private void ViewArchives() => RequestViewArchives?.Invoke();

        /// <summary>Raises an event so the View can open the Settings dialog.</summary>
        [RelayCommand]
        private void OpenSettings() => RequestOpenSettings?.Invoke();

        /// <summary>Raises an event so the View can open the Help dialog.</summary>
        [RelayCommand]
        private void OpenHelp() => RequestOpenHelp?.Invoke();

        // -------------------------------------------------------
        //  Events — raised by commands; the View opens the dialogs
        // -------------------------------------------------------
        public event Action?             RequestAddTask;
        public event Action<KanbanTask>? RequestEditTask;
        public event Action?             RequestViewArchives;
        public event Action?             RequestOpenSettings;
        public event Action?             RequestOpenHelp;

        // -------------------------------------------------------
        //  Public methods called by the View after dialog results
        // -------------------------------------------------------

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

        /// <summary>Loads (or reloads) all active tasks from the database.</summary>
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

        /// <summary>Reads persisted settings and applies them to the application.</summary>
        public void ApplyStartupSettings()
        {
            bool isDark     = _db.GetSetting("DarkMode",   "0") == "1";
            bool showBadges = _db.GetSetting("ShowBadges", "1") == "1";

            new SettingsWindow().ApplyTheme(isDark);
            BadgeVisibility = showBadges ? Visibility.Visible : Visibility.Collapsed;
        }

        // -------------------------------------------------------
        //  Private helpers
        // -------------------------------------------------------

        private void MoveTask(KanbanTask task, string newColumn)
        {
            if (newColumn == task.Column) return;
            string oldColumn = task.Column;
            task.Column = newColumn;
            _db.UpdateTask(task);
            GetCollection(oldColumn).Remove(task);
            GetCollection(newColumn).Add(task);
        }

        // Updates HasTasks and re-evaluates ArchiveAllCommand's CanExecute.
        private void RefreshHasTasks()
        {
            HasTasks = TodoTasks.Count > 0 || InProgressTasks.Count > 0 || DoneTasks.Count > 0;
            ArchiveAllCommand.NotifyCanExecuteChanged();
        }

        private ObservableCollection<KanbanTask> GetCollection(string column) => column switch
        {
            "To Do"       => TodoTasks,
            "In Progress" => InProgressTasks,
            "Done"        => DoneTasks,
            _             => throw new ArgumentException($"Invalid column '{column}'", nameof(column))
        };
    }
}
