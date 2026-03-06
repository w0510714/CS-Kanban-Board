using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfAppLab6Kanban.Models
{
    // EF Core entity + MVVM observable.
    // [ObservableProperty] generates the backing field, public property,
    // and OnPropertyChanged call — no manual boilerplate needed.
    public partial class KanbanTask : ObservableObject
    {
        [Key]
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _priority = "Medium";

        // "Column" is a SQL reserved word — [Column("Column")] ensures EF Core quotes it.
        [Column("Column")]
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
