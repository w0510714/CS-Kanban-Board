using Microsoft.EntityFrameworkCore;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.Data
{
    // DbContext bridges EF Core to the SQLite database.
    // DbSet<T> properties represent tables; LINQ queries translate to SQL automatically.
    public class KanbanDbContext : DbContext
    {
        /// <summary>The Tasks table — active and archived Kanban cards.</summary>
        public DbSet<KanbanTask> Tasks { get; set; } = null!;

        /// <summary>The Settings table — persisted key/value app preferences.</summary>
        public DbSet<Setting> Settings { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = System.IO.Path.Combine(
                System.AppContext.BaseDirectory, "kanban.db");

            optionsBuilder
                .UseSqlite($"Data Source={dbPath}")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KanbanTask>(entity =>
            {
                entity.ToTable("Tasks");
                entity.Property(t => t.Id).ValueGeneratedOnAdd();

                // "Column" is a SQL reserved word — HasColumnName ensures EF Core quotes it.
                entity.Property(t => t.Column)
                      .HasColumnName("Column")
                      .HasDefaultValue("To Do")
                      .IsRequired();

                entity.Property(t => t.Priority)
                      .HasDefaultValue("Medium")
                      .IsRequired();
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.ToTable("Settings");
                entity.HasKey(s => s.Key);
            });
        }
    }
}
