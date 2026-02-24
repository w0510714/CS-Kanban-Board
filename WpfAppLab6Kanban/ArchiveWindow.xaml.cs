using System.Collections.ObjectModel;
using System.Windows;
using WpfAppLab6Kanban.Data;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class ArchiveWindow : Window
    {
        private readonly DatabaseService _db = new DatabaseService();
        public ObservableCollection<KanbanTask> ArchivedTasks { get; set; } = new ObservableCollection<KanbanTask>();

        public ArchiveWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadArchivedTasks();
        }

        private void LoadArchivedTasks()
        {
            ArchivedTasks.Clear();
            var tasks = _db.GetArchivedTasks();
            foreach (var task in tasks)
            {
                ArchivedTasks.Add(task);
            }
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (ArchiveListView.SelectedItem is KanbanTask selectedTask)
            {
                _db.RestoreTask(selectedTask.Id);
                ArchivedTasks.Remove(selectedTask);
                MessageBox.Show($"'{selectedTask.Title}' restored to the board.", "Restored", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a task to restore.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ArchiveListView.SelectedItem is KanbanTask selectedTask)
            {
                var result = MessageBox.Show($"Are you sure you want to PERMANENTLY delete '{selectedTask.Title}'? This cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _db.DeleteTask(selectedTask.Id);
                    ArchivedTasks.Remove(selectedTask);
                }
            }
            else
            {
                MessageBox.Show("Please select a task to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (ArchivedTasks.Count == 0) return;

            var result = MessageBox.Show("Are you sure you want to PERMANENTLY delete ALL archived tasks? This cannot be undone.", "Confirm Cleanup", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                _db.DeleteAllArchived();
                ArchivedTasks.Clear();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
