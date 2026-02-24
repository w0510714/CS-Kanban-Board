using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _db = new DatabaseService();

        public ObservableCollection<KanbanTask> TodoTasks { get; set; } = new ObservableCollection<KanbanTask>();
        public ObservableCollection<KanbanTask> InProgressTasks { get; set; } = new ObservableCollection<KanbanTask>();
        public ObservableCollection<KanbanTask> DoneTasks { get; set; } = new ObservableCollection<KanbanTask>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadTasks();
        }

        private void LoadTasks()
        {
            TodoTasks.Clear();
            InProgressTasks.Clear();
            DoneTasks.Clear();

            var allTasks = _db.GetAllTasks();
            foreach (var task in allTasks)
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
                        // Update DB (handles Title/Priority/Status changes)
                        _db.UpdateTask(selectedTask);

                        // If moved or archived, shift collections
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
            var archiveWindow = new ArchiveWindow { Owner = this };
            archiveWindow.ShowDialog();
            
            LoadTasks();
        }

        private void ArchiveAll_Click(object sender, RoutedEventArgs e)
        {
            if (TodoTasks.Count == 0 && InProgressTasks.Count == 0 && DoneTasks.Count == 0)
            {
                MessageBox.Show("There are no tasks to archive.", "Board Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to archive all tasks on the board? This ends the current sprint.", "Archive All", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _db.ArchiveAllTasks();
                TodoTasks.Clear();
                InProgressTasks.Clear();
                DoneTasks.Clear();
                UpdateArchiveButtonState();
            }
        }

        private void UpdateArchiveButtonState()
        {
            bool hasTasks = TodoTasks.Count > 0 || InProgressTasks.Count > 0 || DoneTasks.Count > 0;
            ArchiveAllButton.IsEnabled = hasTasks;
            ArchiveAllButton.Opacity = hasTasks ? 1.0 : 0.5;
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
            _ => throw new System.ArgumentException("Invalid column name")
        };

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is KanbanTask task)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{task.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _db.DeleteTask(task.Id);
                    GetCollection(task.Column).Remove(task);
                    UpdateArchiveButtonState();
                }
            }
        }
    }
}