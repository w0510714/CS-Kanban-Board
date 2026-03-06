using System.ComponentModel.DataAnnotations;

namespace WpfAppLab6Kanban.Models
{
    // ======================================================================
    //  Setting — EF Core entity for the Settings table
    // ======================================================================
    //
    //  The Settings table is a simple key/value store used to persist app
    //  preferences (DarkMode, ShowBadges, etc.) between sessions.
    //
    //  Previously the DatabaseService used raw SQL INSERT ... ON CONFLICT
    //  to upsert settings.  EF Core replaces that with:
    //      context.Settings.Update(setting);   // upsert via change tracker
    //      context.SaveChanges();
    // ======================================================================
    public class Setting
    {
        // String primary key — EF Core uses this as the WHERE clause in UPDATE/DELETE
        [Key]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
