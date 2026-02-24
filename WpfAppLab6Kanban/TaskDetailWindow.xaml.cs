using System;
using System.Windows;
using System.Windows.Controls;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban
{
    public partial class TaskDetailWindow : Window
    {
        public KanbanTask Task { get; private set; }
        public bool IsDeleted { get; private set; }

        public TaskDetailWindow(KanbanTask task)
        {
            InitializeComponent();
            Task = task;
            
            LoadTaskData();
            ApplySecurityRules();

            // Wire up change tracking after initial load
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

            // Set Priority ComboBox
            foreach (ComboBoxItem item in PriorityComboBox.Items)
            {
                if (item.Content.ToString() == Task.Priority)
                {
                    PriorityComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ApplySecurityRules()
        {
            if (Task.IsArchived)
            {
                // Archived tasks are READ-ONLY
                TitleTextBox.IsReadOnly = true;
                DescriptionTextBox.IsReadOnly = true;
                PriorityComboBox.IsEnabled = false;
                SaveButton.IsEnabled = false;
                MoveGroupBox.Visibility = Visibility.Collapsed;
                Title = "Archived Task Details (Read Only)";
            }
            else
            {
                // Active tasks: Visibility rules for Move buttons
                MoveTodoBtn.Visibility = (Task.Column == "In Progress") ? Visibility.Visible : Visibility.Collapsed;
                MoveProgressBtn.Visibility = (Task.Column == "To Do" || Task.Column == "Done") ? Visibility.Visible : Visibility.Collapsed;
                MoveDoneBtn.Visibility = (Task.Column == "In Progress") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var result = MessageBox.Show($"Are you sure you want to PERMANENTLY delete '{Task.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                IsDeleted = true;
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
