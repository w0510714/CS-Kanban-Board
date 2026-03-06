using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfAppLab6Kanban.Models
{
    // Inheriting ObservableObject gives us INotifyPropertyChanged for free.
    // The [ObservableProperty] attribute generates the private backing field,
    // the public property, and the OnPropertyChanged call — no boilerplate required.
    public partial class KanbanTask : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _priority = "Medium";

        // Kanban column: "To Do", "In Progress", or "Done"
        [ObservableProperty]
        private string _column = "To Do";

        [ObservableProperty]
        private int _position;

        [ObservableProperty]
        private bool _isArchived;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private DateTime _updatedAt;
    }
}
