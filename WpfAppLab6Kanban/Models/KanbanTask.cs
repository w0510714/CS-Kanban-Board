using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppLab6Kanban.Models
{
    // Represents a single task with property-change notification for the UI
    public class KanbanTask : INotifyPropertyChanged
    {
        private int _id;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _priority = "Medium";
        private string _column = "To Do";
        private int _position;
        private bool _isArchived;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        // SQLite primary key
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // Kanban status: "To Do", "In Progress", or "Done"
        public string Column
        {
            get => _column;
            set { _column = value; OnPropertyChanged(); }
        }

        public int Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        public bool IsArchived
        {
            get => _isArchived;
            set { _isArchived = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set { _updatedAt = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Notifies the UI to refresh when a property changes
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
