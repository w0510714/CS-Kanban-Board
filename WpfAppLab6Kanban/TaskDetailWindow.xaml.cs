using System;
using System.Windows;
using System.Windows.Controls;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class TaskDetailWindow : Window
    {
        public KanbanTask Task { get; private set; }
        public bool IsDeleted { get; private set; } // Flag to tell parent window task was deleted

        public TaskDetailWindow(KanbanTask task)
        {
            InitializeComponent();
            Task = task;
            
            LoadTaskData();
            ApplySecurityRules();

            // Notify user only when they actually change something
            TitleTextBox.TextChanged += (s, e) => SaveButton.IsEnabled = true;
            DescriptionTextBox.TextChanged += (s, e) => SaveButton.IsEnabled = true;
            PriorityComboBox.SelectionChanged += (s, e) => SaveButton.IsEnabled = true;
        }

        private void LoadTaskData()
        {
            TitleTextBox.Text = Task.Title;
            DescriptionTextBox.Text = Task.Description;
            StatusLabel.Text = $"Status: {Task.Column.ToUpper()}";
            ArchivedLabel.Text = $"Archived: {(Task.IsArchived ? "YES" : "NO")}";

            foreach (ComboBoxItem item in PriorityComboBox.Items)
            {
                if (item.Content.ToString() == Task.Priority)
                {
                    PriorityComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        // Adjusts UI based on archived status and board rules
        private void ApplySecurityRules()
        {
            if (Task.IsArchived)
            {
                // Archived tasks are read-only
                TitleTextBox.IsReadOnly = true;
                DescriptionTextBox.IsReadOnly = true;
                PriorityComboBox.IsEnabled = false;
                SaveButton.IsEnabled = false;
                MoveGroupBox.Visibility = Visibility.Collapsed;
                Title = "Archived Task Details (Read Only)";
            }
            else
            {
                // Show movement buttons only if they represent a valid next step
                MoveTodoBtn.Visibility = (Task.Column == "In Progress") ? Visibility.Visible : Visibility.Collapsed;
                MoveProgressBtn.Visibility = (Task.Column == "To Do" || Task.Column == "Done") ? Visibility.Visible : Visibility.Collapsed;
                MoveDoneBtn.Visibility = (Task.Column == "In Progress") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Task.Title = TitleTextBox.Text.Trim();
            Task.Description = DescriptionTextBox.Text.Trim();
            Task.Priority = (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Medium";
            Task.UpdatedAt = DateTime.UtcNow;

            DialogResult = true;
            Close();
        }

        private void MoveTodo_Click(object sender, RoutedEventArgs e)
        {
            Task.Column = "To Do";
            DialogResult = true;
            Close();
        }

        private void MoveProgress_Click(object sender, RoutedEventArgs e)
        {
            Task.Column = "In Progress";
            DialogResult = true;
            Close();
        }

        private void MoveDone_Click(object sender, RoutedEventArgs e)
        {
            Task.Column = "Done";
            DialogResult = true;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Permanently delete '{Task.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                IsDeleted = true; // parent must handle actual DB deletion
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
