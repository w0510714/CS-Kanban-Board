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
            string newColumn = task.Column switch
            {
                "In Progress" => "To Do",
                "Done"        => "In Progress",
                _             => task.Column
            };
            MoveTask(task, newColumn);
        }

        /// <summary>Moves a task one column to the right.</summary>
        [RelayCommand]
        private void MoveRight(KanbanTask task)
        {
            if (task == null) return;
            string newColumn = task.Column switch
            {
                "To Do"       => "In Progress",
                "In Progress" => "Done",
                _             => task.Column
            };
            MoveTask(task, newColumn);
        }

        /// <summary>Permanently deletes a task after confirmation.</summary>
        [RelayCommand]
        private void DeleteTask(KanbanTask task)
        {
            if (task == null) return;
            if (MessageBox.Show($"Permanently delete '{task.Title}'?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _db.DeleteTask(task.Id);
                GetCollection(task.Column).Remove(task);
                RefreshHasTasks();
            }
        }

        /// <summary>Archives every active task (ends the sprint).</summary>
        [RelayCommand(CanExecute = nameof(HasTasks))]
        private void ArchiveAll()
        {
            if (MessageBox.Show("Archive all tasks and clear the board?", "Confirm",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _db.ArchiveAllTasks();
                LoadTasks();
            }
        }

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

        private void MoveTask(KanbanTask task, string newColumn)
        {
            if (newColumn == task.Column) return;
            string oldColumn = task.Column;
            task.Column = newColumn;
            _db.UpdateTask(task);
            GetCollection(oldColumn).Remove(task);
            GetCollection(newColumn).Add(task);
        }

        private void RefreshHasTasks()
        {
            HasTasks = TodoTasks.Count > 0 || InProgressTasks.Count > 0 || DoneTasks.Count > 0;
            // Re-evaluate the CanExecute of ArchiveAllCommand
            ArchiveAllCommand.NotifyCanExecuteChanged();
        }

        private ObservableCollection<KanbanTask> GetCollection(string column) => column switch
        {
            "To Do"       => TodoTasks,
            "In Progress" => InProgressTasks,
            "Done"        => DoneTasks,
            _             => throw new ArgumentException("Invalid column name", nameof(column))
        };
    }
}
