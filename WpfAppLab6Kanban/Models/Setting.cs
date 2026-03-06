using System.ComponentModel.DataAnnotations;

namespace WpfAppLab6Kanban.Models
{
    public class Setting
    {
        // String primary key — EF Core uses this as the WHERE clause in UPDATE/DELETE
        [Key]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
