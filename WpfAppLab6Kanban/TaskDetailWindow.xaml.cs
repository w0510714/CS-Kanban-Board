using System.Windows;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class TaskDetailWindow : Window
    {
        public KanbanTask Task { get; private set; }
        public bool IsSaved { get; private set; }

        public TaskDetailWindow(KanbanTask task)
        {
            InitializeComponent();
            Task = task;
            this.DataContext = Task;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.Title))
            {
                MessageBox.Show("Please enter a title.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaved = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
