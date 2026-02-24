using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppLab6Kanban.Models
{
    /// <summary>
    /// Represents a single task card on the Kanban board.
    /// Implements INotifyPropertyChanged so the WPF UI automatically
    /// refreshes whenever a property value changes.
    /// </summary>
    public class KanbanTask : INotifyPropertyChanged
    {
        // ── Backing fields ──────────────────────────────────────────────────────
        private int    _id;
        private string _title       = string.Empty;
        private string _description = string.Empty;
        private string _column      = "To Do";   // "To Do" | "In Progress" | "Done"
        private int    _position;                  // display order within a column
        private DateTime _createdAt;
        private DateTime _updatedAt;

        // ── Properties ──────────────────────────────────────────────────────────

        /// <summary>Primary key stored in SQLite.</summary>
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        /// <summary>Short headline for the task card.</summary>
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        /// <summary>Optional longer description / notes for the task.</summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Which Kanban column this task belongs to.
        /// Valid values: "To Do", "In Progress", "Done"
        /// </summary>
        public string Column
        {
            get => _column;
            set { _column = value; OnPropertyChanged(); }
        }

        /// <summary>Zero-based sort order within the column.</summary>
        public int Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        /// <summary>UTC timestamp when the task was first created.</summary>
        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        /// <summary>UTC timestamp of the most recent update.</summary>
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set { _updatedAt = value; OnPropertyChanged(); }
        }

        // ── INotifyPropertyChanged ───────────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises PropertyChanged. The [CallerMemberName] attribute automatically
        /// fills in the name of the property that called this method,
        /// so you never have to type a magic string like "Title" manually.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
