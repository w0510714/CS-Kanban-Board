using System.Windows;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class AddTaskWindow : Window
    {
        public KanbanTask? NewTask { get; private set; }

        public AddTaskWindow()
        {
            InitializeComponent();
            TitleTextBox.Focus();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a task title.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewTask = new KanbanTask
            {
                Title = TitleTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim(),
                Column = "To Do",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

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
