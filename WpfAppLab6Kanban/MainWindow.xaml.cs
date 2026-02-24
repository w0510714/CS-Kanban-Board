using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DatabaseService _db;
        private Visibility _badgeVisibility = Visibility.Visible;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Visibility BadgeVisibility
        {
            get => _badgeVisibility;
            set { _badgeVisibility = value; OnPropertyChanged(); }
        }

        public ObservableCollection<KanbanTask> TodoTasks { get; set; } = new ObservableCollection<KanbanTask>();
        public ObservableCollection<KanbanTask> InProgressTasks { get; set; } = new ObservableCollection<KanbanTask>();
        public ObservableCollection<KanbanTask> DoneTasks { get; set; } = new ObservableCollection<KanbanTask>();

        public MainWindow()
        {
            InitializeComponent();
            _db = new DatabaseService();
            ApplyStartupSettings();
            this.DataContext = this;
            LoadTasks();
        }

        private void ApplyStartupSettings()
        {
            bool isDark = _db.GetSetting("DarkMode", "0") == "1";
            new SettingsWindow().ApplyTheme(isDark);

            bool showBadges = _db.GetSetting("ShowBadges", "1") == "1";
            BadgeVisibility = showBadges ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            HamburgerButton.ContextMenu.IsOpen = true;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (new SettingsWindow { Owner = this }.ShowDialog() == true)
            {
                ApplyStartupSettings();
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            new HelpWindow { Owner = this }.ShowDialog();
        }

        private void LoadTasks()
        {
            TodoTasks.Clear();
            InProgressTasks.Clear();
            DoneTasks.Clear();

            foreach (var task in _db.GetAllTasks())
            {
                switch (task.Column)
                {
                    case "To Do": TodoTasks.Add(task); break;
                    case "In Progress": InProgressTasks.Add(task); break;
                    case "Done": DoneTasks.Add(task); break;
                }
            }
            UpdateArchiveButtonState();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddTaskWindow { Owner = this };
            if (addWindow.ShowDialog() == true && addWindow.NewTask != null)
            {
                _db.AddTask(addWindow.NewTask);
                TodoTasks.Add(addWindow.NewTask);
                UpdateArchiveButtonState();
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is KanbanTask selectedTask)
            {
                string originalColumn = selectedTask.Column;
                var detailWindow = new TaskDetailWindow(selectedTask) { Owner = this };
                
                if (detailWindow.ShowDialog() == true)
                {
                    if (detailWindow.IsDeleted)
                    {
                        _db.DeleteTask(selectedTask.Id);
                        GetCollection(originalColumn).Remove(selectedTask);
                    }
                    else
                    {
                        _db.UpdateTask(selectedTask);
                        if (selectedTask.Column != originalColumn)
                        {
                            GetCollection(originalColumn).Remove(selectedTask);
                            GetCollection(selectedTask.Column).Add(selectedTask);
                        }
                        if (selectedTask.IsArchived)
                        {
                            GetCollection(selectedTask.Column).Remove(selectedTask);
                        }
                    }
                    UpdateArchiveButtonState();
                }
            }
        }

        private void ViewArchives_Click(object sender, RoutedEventArgs e)
        {
            new ArchiveWindow { Owner = this }.ShowDialog();
            LoadTasks();
        }

        private void ArchiveAll_Click(object sender, RoutedEventArgs e)
        {
            if (TodoTasks.Count == 0 && InProgressTasks.Count == 0 && DoneTasks.Count == 0) return;
            if (MessageBox.Show("Archive all tasks and clear the board?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _db.ArchiveAllTasks();
                LoadTasks();
            }
        }

        private void UpdateArchiveButtonState()
        {
            bool hasTasks = TodoTasks.Count > 0 || InProgressTasks.Count > 0 || DoneTasks.Count > 0;
            if (ArchiveAllMenuItem != null) ArchiveAllMenuItem.IsEnabled = hasTasks;
        }

        private void MoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is KanbanTask task)
            {
                string oldColumn = task.Column;
                string newColumn = oldColumn switch
                {
                    "In Progress" => "To Do",
                    "Done" => "In Progress",
                    _ => oldColumn
                };
                if (newColumn != oldColumn)
                {
                    task.Column = newColumn;
                    _db.UpdateTask(task);
                    MoveTaskInUI(task, oldColumn, newColumn);
                }
            }
        }

        private void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is KanbanTask task)
            {
                string oldColumn = task.Column;
                string newColumn = oldColumn switch
                {
                    "To Do" => "In Progress",
                    "In Progress" => "Done",
                    _ => oldColumn
                };
                if (newColumn != oldColumn)
                {
                    task.Column = newColumn;
                    _db.UpdateTask(task);
                    MoveTaskInUI(task, oldColumn, newColumn);
                }
            }
        }

        private void MoveTaskInUI(KanbanTask task, string from, string to)
        {
            GetCollection(from).Remove(task);
            GetCollection(to).Add(task);
        }

        private ObservableCollection<KanbanTask> GetCollection(string columnName) => columnName switch
        {
            "To Do" => TodoTasks,
            "In Progress" => InProgressTasks,
            "Done" => DoneTasks,
            _ => throw new ArgumentException("Invalid column name")
        };

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is KanbanTask task)
            {
                if (MessageBox.Show($"Permanently delete '{task.Title}'?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _db.DeleteTask(task.Id);
                    GetCollection(task.Column).Remove(task);
                    UpdateArchiveButtonState();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}