using System.Windows;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class TaskDetailWindow : Window
    {
        public KanbanTask Task { get; private set; }
        
        public TaskDetailWindow(KanbanTask task)
        {
            InitializeComponent();
            Task = task;
            
            // Fill textboxes with current values
            TitleTextBox.Text = task.Title;
            DescriptionTextBox.Text = task.Description;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a title.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Apply changes back to the task object
            Task.Title = TitleTextBox.Text;
            Task.Description = DescriptionTextBox.Text;
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to archive this task?", "Confirm Archive", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Task.IsArchived = true;
                DialogResult = true;
                Close();
            }
        }
    }
}
