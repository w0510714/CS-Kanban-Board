using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.ViewModels
{
    // ------------------------------------------------------------------
    // MainViewModel — the single source of truth for the Kanban board.
    //
    // MVVM pattern roles:
    //   Model      → KanbanTask, DatabaseService  (data / business logic)
    //   View       → MainWindow.xaml              (pure UI markup)
    //   ViewModel  → THIS CLASS                   (state + commands)
    //
    // The View binds its controls to properties and commands defined here.
    // The ViewModel never imports any WPF Window/Control types — it stays
    // fully testable without a running UI.
    // ------------------------------------------------------------------
    public class MainViewModel : INotifyPropertyChanged
    {
        // ──────────────────────────────────────────────────────────────
        // Infrastructure
        // ──────────────────────────────────────────────────────────────
        private readonly DatabaseService _db;

        // The View subscribes to this event through data-binding so the
        // UI refreshes automatically whenever a property value changes.
        public event PropertyChangedEventHandler? PropertyChanged;

        // ──────────────────────────────────────────────────────────────
        // Observable collections — bound to the three ListBox controls
        // ──────────────────────────────────────────────────────────────
        public ObservableCollection<KanbanTask> TodoTasks       { get; } = new();
        public ObservableCollection<KanbanTask> InProgressTasks { get; } = new();
        public ObservableCollection<KanbanTask> DoneTasks       { get; } = new();

        // ──────────────────────────────────────────────────────────────
        // BadgeVisibility — controls the priority badge in the card template
        // ──────────────────────────────────────────────────────────────
        private Visibility _badgeVisibility = Visibility.Visible;

        public Visibility BadgeVisibility
        {
            get => _badgeVisibility;
            set { _badgeVisibility = value; OnPropertyChanged(); }
        }

        // ──────────────────────────────────────────────────────────────
        // Commands — each one replaces a Click event-handler method that
        // used to live in MainWindow.xaml.cs
        // ──────────────────────────────────────────────────────────────

        // Opens the Add-Task dialog (the View wires the window-opening logic
        // via the RequestAddTask event below — see Event delegation section).
        public ICommand AddTaskCommand       { get; }

        // Moves a card one column to the left
        public ICommand MoveLeftCommand      { get; }

        // Moves a card one column to the right
        public ICommand MoveRightCommand     { get; }

        // Permanently deletes a card after confirmation
        public ICommand DeleteTaskCommand    { get; }

        // Archives every active task ("End Sprint")
        public ICommand ArchiveAllCommand    { get; }

        // ──────────────────────────────────────────────────────────────
        // Event delegation — the ViewModel needs to open child windows
        // but must NOT reference any WPF Window type directly.  Instead
        // it raises plain .NET events; the View subscribes and opens the
        // appropriate dialog.
        // ──────────────────────────────────────────────────────────────
        public event Action?            RequestAddTask;
        public event Action<KanbanTask>? RequestEditTask;
        public event Action?            RequestViewArchives;
        public event Action?            RequestOpenSettings;
        public event Action?            RequestOpenHelp;

        // ──────────────────────────────────────────────────────────────
        // Constructor
        // ──────────────────────────────────────────────────────────────
        public MainViewModel(DatabaseService db)
        {
            _db = db;

            // Wire up every command to its handler method
            AddTaskCommand    = new RelayCommand(OnAddTask);
            MoveLeftCommand   = new RelayCommand<KanbanTask>(OnMoveLeft,  t => t?.Column != "To Do");
            MoveRightCommand  = new RelayCommand<KanbanTask>(OnMoveRight, t => t?.Column != "Done");
            DeleteTaskCommand = new RelayCommand<KanbanTask>(OnDeleteTask);
            ArchiveAllCommand = new RelayCommand(OnArchiveAll,
                                    () => TodoTasks.Count > 0 || InProgressTasks.Count > 0 || DoneTasks.Count > 0);

            ApplyStartupSettings();
            LoadTasks();
        }

        // ──────────────────────────────────────────────────────────────
        // Public methods called by the View after dialog results
        // ──────────────────────────────────────────────────────────────

        // Called by MainWindow after the Add-Task dialog returns a new task
        public void CommitNewTask(KanbanTask task)
        {
            _db.AddTask(task);
            TodoTasks.Add(task);
        }

        // Called by MainWindow after the Edit-Task dialog finishes
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

                // Move between columns if the user changed the status
                if (task.Column != originalColumn)
                {
                    GetCollection(originalColumn).Remove(task);
                    GetCollection(task.Column).Add(task);
                }

                // Remove from the board if it was just archived
                if (task.IsArchived)
                    GetCollection(task.Column).Remove(task);
            }
        }

        // Reload everything from the database (called after the Archive window closes)
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
        }

        // Re-read settings from the database and apply them
        public void ApplyStartupSettings()
        {
            bool showBadges  = _db.GetSetting("ShowBadges", "1") == "1";
            BadgeVisibility  = showBadges ? Visibility.Visible : Visibility.Collapsed;
        }

        // ──────────────────────────────────────────────────────────────
        // Command handler methods (private — the View never calls these)
        // ──────────────────────────────────────────────────────────────

        private void OnAddTask() => RequestAddTask?.Invoke();

        private void OnMoveLeft(KanbanTask? task)
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

        private void OnMoveRight(KanbanTask? task)
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

        private void OnDeleteTask(KanbanTask? task)
        {
            if (task is null) return;

            var result = MessageBox.Show(
                $"Permanently delete '{task.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _db.DeleteTask(task.Id);
                GetCollection(task.Column).Remove(task);
            }
        }

        private void OnArchiveAll()
        {
            var result = MessageBox.Show(
                "Archive all tasks and clear the board?",
                "End Sprint",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _db.ArchiveAllTasks();
                LoadTasks();
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Public trigger methods — let the View raise ViewModel events
        // without violating C#'s event encapsulation rules.
        // (Events can only be invoked from within their declaring class.)
        // ──────────────────────────────────────────────────────────────
        public void OpenArchives()          => RequestViewArchives?.Invoke();
        public void OpenSettings()          => RequestOpenSettings?.Invoke();
        public void OpenHelp()              => RequestOpenHelp?.Invoke();
        public void RaiseEditTask(KanbanTask task) => RequestEditTask?.Invoke(task);

        // ──────────────────────────────────────────────────────────────
        // Internal helpers
        // ──────────────────────────────────────────────────────────────
        private void MoveTask(KanbanTask task, string newColumn)
        {
            if (newColumn == task.Column) return;

            string oldColumn = task.Column;
            task.Column = newColumn;
            _db.UpdateTask(task);
            GetCollection(oldColumn).Remove(task);
            GetCollection(newColumn).Add(task);
        }

        private ObservableCollection<KanbanTask> GetCollection(string column) => column switch
        {
            "To Do"       => TodoTasks,
            "In Progress" => InProgressTasks,
            "Done"        => DoneTasks,
            _             => throw new ArgumentException($"Unknown column '{column}'")
        };

        // Raises PropertyChanged so bound controls update automatically
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ------------------------------------------------------------------
    // RelayCommand<T> — typed variant for commands that receive a strongly-
    // typed parameter (e.g., a KanbanTask) via CommandParameter binding.
    // ------------------------------------------------------------------
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?>    _execute;
        private readonly Func<T?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute    = execute    ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute is null) return true;
            return parameter is T t ? _canExecute(t) : _canExecute(default);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter is T t ? t : default);
        }
    }
}
