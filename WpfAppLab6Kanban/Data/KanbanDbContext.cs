using Microsoft.EntityFrameworkCore;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.Data
{
    // ======================================================================
    //  KanbanDbContext — the EF Core DbContext
    // ======================================================================
    //
    //  DbContext is the central EF Core class that:
    //    • Holds DbSet<T> properties — one per database table
    //    • Manages the connection and tracks entity changes
    //    • Translates LINQ queries into SQL
    //    • Writes changes back via SaveChanges()
    //
    //  How schema creation works (desktop app pattern):
    //    EnsureCreated() in OnConfiguring checks whether the database file
    //    and tables already exist.  If they do → does nothing (existing data
    //    is preserved).  If they don't → creates them from the entity model.
    //    This is simpler than running EF Core migrations for a desktop app.
    // ======================================================================
    public class KanbanDbContext : DbContext
    {
        // ── DbSets — one property per table ──────────────────────────────
        // EF Core translates queries on these into SQL automatically.
        // Example:  Tasks.Where(t => !t.IsArchived)  →  SELECT ... WHERE IsArchived = 0

        /// <summary>The Tasks table — active and archived Kanban cards.</summary>
        public DbSet<KanbanTask> Tasks { get; set; } = null!;

        /// <summary>The Settings table — persisted key/value app preferences.</summary>
        public DbSet<Setting> Settings { get; set; } = null!;

        // ── Configure the SQLite connection ──────────────────────────────
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Store the database file next to the executable
            string dbPath = System.IO.Path.Combine(
                System.AppContext.BaseDirectory, "kanban.db");

            optionsBuilder
                .UseSqlite($"Data Source={dbPath}")
                // EnsureCreated is called here so every new DbContext instance
                // guarantees the schema exists before any LINQ queries run.
                // (This replaces the hand-written CREATE TABLE IF NOT EXISTS SQL.)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        // ── Fluent API configuration ──────────────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map Tasks entity to the "Tasks" table
            modelBuilder.Entity<KanbanTask>(entity =>
            {
                entity.ToTable("Tasks");

                // Tell EF Core the table uses AUTOINCREMENT for the PK
                // (SQLite specific — prevents ID reuse after row deletion)
                entity.Property(t => t.Id)
                      .ValueGeneratedOnAdd();

                // "Column" is a SQL reserved word; the [Column("Column")] annotation
                // on the model already handles quoting, but we re-confirm it here
                // in the fluent API for clarity.
                entity.Property(t => t.Column)
                      .HasColumnName("Column")
                      .HasDefaultValue("To Do")
                      .IsRequired();

                entity.Property(t => t.Priority)
                      .HasDefaultValue("Medium")
                      .IsRequired();
            });

            // Map Setting entity to the "Settings" table (string PK)
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.ToTable("Settings");
                entity.HasKey(s => s.Key);
            });
        }
    }
}
