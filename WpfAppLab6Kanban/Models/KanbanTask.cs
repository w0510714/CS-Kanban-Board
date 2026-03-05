using System;
<<<<<<< HEAD
<<<<<<< HEAD
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfAppLab6Kanban.Models
{
<<<<<<< HEAD
<<<<<<< HEAD
    // ======================================================================
    //  KanbanTask — Model (EF Core entity + MVVM observable)
    // ======================================================================
    //
    //  This class serves two roles at once:
    //
    //    1. EF Core entity  — EF reads/writes the Tasks table using this class.
    //       Data annotations configure how properties map to database columns:
    //         [Key]              → marks the primary key (Id)
    //         [Column("Column")] → maps the C# property to the SQL column name
    //                              "Column" is a SQL reserved word, so we must
    //                              tell EF Core the exact column name to quote it.
    //
    //    2. MVVM observable  — ObservableObject + [ObservableProperty] generate
    //       INotifyPropertyChanged so the UI refreshes when data changes.
    //       (See Topic 2 — MVVM Toolkit)
    // ======================================================================
    public partial class KanbanTask : ObservableObject
    {
        // Primary key — EF Core uses this to track and update the record
        [Key]
=======
    // Inheriting ObservableObject gives us INotifyPropertyChanged for free.
    // The [ObservableProperty] attribute generates the private backing field,
    // the public property, and the OnPropertyChanged call — no boilerplate required.
    public partial class KanbanTask : ObservableObject
    {
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
    // Inheriting ObservableObject gives us INotifyPropertyChanged for free.
    // The [ObservableProperty] attribute generates the private backing field,
    // the public property, and the OnPropertyChanged call — no boilerplate required.
    public partial class KanbanTask : ObservableObject
    {
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _priority = "Medium";

<<<<<<< HEAD
<<<<<<< HEAD
        // "Column" is a SQL reserved word.
        // [Column("Column")] tells EF Core to use that exact name (quoted).
        // This preserves compatibility with the existing kanban.db schema.
        [Column("Column")]
=======
        // Kanban column: "To Do", "In Progress", or "Done"
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
=======
        // Kanban column: "To Do", "In Progress", or "Done"
>>>>>>> c46f990 (Task 1: Adopt MVVM architecture with Commands and MVVM Toolkit)
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
