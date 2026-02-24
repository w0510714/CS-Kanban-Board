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
            foreach (var task in _db.GetArchivedTasks())
            {
                ArchivedTasks.Add(task);
            }
        }

        private void ArchiveListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ArchiveListView.SelectedItem is KanbanTask selectedTask)
            {
                // Open archived tasks in read-only mode
                var detailWindow = new TaskDetailWindow(selectedTask) { Owner = this };
                if (detailWindow.ShowDialog() == true && detailWindow.IsDeleted)
                {
                    _db.DeleteTask(selectedTask.Id);
                    ArchivedTasks.Remove(selectedTask);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ArchiveListView.SelectedItem is KanbanTask selectedTask)
            {
                if (MessageBox.Show($"Permanently delete '{selectedTask.Title}'?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _db.DeleteTask(selectedTask.Id);
                    ArchivedTasks.Remove(selectedTask);
                }
            }
        }

        // Nuclear option: clear everything in the archive
        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (ArchivedTasks.Count == 0) return;

            if (MessageBox.Show("Permanently delete ALL archived tasks?", "Final Confirm", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                _db.DeleteAllArchived();
                ArchivedTasks.Clear();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
