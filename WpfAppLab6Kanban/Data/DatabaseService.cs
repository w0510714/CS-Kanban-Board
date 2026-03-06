using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.Data
{
    // ======================================================================
    //  DatabaseService — EF Core edition
    // ======================================================================
    //
    //  BEFORE (raw Microsoft.Data.Sqlite):
    //    • Hand-written SQL strings for every operation
    //    • Manual SqliteConnection + SqliteCommand + SqliteDataReader
    //    • Manual parameter binding  (.Parameters.AddWithValue(...))
    //    • Manual row mapping        (MapRow(SqliteDataReader reader))
    //    • Manual null checks        (reader.IsDBNull(5) ? ... : ...)
    //    • 281 lines total
    //
    //  AFTER (Entity Framework Core):
    //    • LINQ queries instead of SQL strings — type-safe, refactorable
    //    • EF Core manages the connection lifecycle automatically
    //    • No parameter binding — EF Core parameterizes everything
    //    • No row mapping — EF Core materializes entities directly
    //    • EnsureCreated() replaces CREATE TABLE IF NOT EXISTS SQL
    //    • ~100 lines total
    //
    //  Pattern used: short-lived DbContext per operation ("unit of work").
    //  Each public method creates its own context, does one operation, then
    //  disposes it.  This is the recommended pattern for desktop WPF apps
    //  because it avoids stale change-tracker state between operations.
    // ======================================================================
    public class DatabaseService
    {
        public DatabaseService()
        {
            // EnsureCreated creates the database file and tables if they do
            // not yet exist.  If kanban.db is already present (from the raw
            // Sqlite version), it is left untouched — no data is lost.
            using var ctx = new KanbanDbContext();
            ctx.Database.EnsureCreated();
        }

        // ── Settings ──────────────────────────────────────────────────────

        /// <summary>Returns a persisted setting value, or defaultValue if not found.</summary>
        public string GetSetting(string key, string defaultValue)
        {
            using var ctx = new KanbanDbContext();
            // FirstOrDefault translates to: SELECT Value FROM Settings WHERE Key = @key LIMIT 1
            return ctx.Settings.FirstOrDefault(s => s.Key == key)?.Value ?? defaultValue;
        }

        /// <summary>Upserts (insert or update) a setting value.</summary>
        public void SaveSetting(string key, string value)
        {
            using var ctx = new KanbanDbContext();

            // Find existing or create new — EF Core tracks which SQL to emit
            var existing = ctx.Settings
                              .AsTracking()           // need tracking to detect Add vs Update
                              .FirstOrDefault(s => s.Key == key);

            if (existing is null)
                ctx.Settings.Add(new Setting { Key = key, Value = value });
            else
                existing.Value = value;

            ctx.SaveChanges();   // EF emits INSERT or UPDATE as needed
        }

        // ── Task CRUD ─────────────────────────────────────────────────────

        /// <summary>
        /// Inserts a new task into the database.
        /// EF Core sets task.Id automatically after SaveChanges().
        /// </summary>
        public KanbanTask AddTask(KanbanTask task)
        {
            task.CreatedAt = task.UpdatedAt = DateTime.UtcNow;

            using var ctx = new KanbanDbContext();
            ctx.Tasks.Add(task);
            ctx.SaveChanges();       // EF emits: INSERT INTO Tasks (...) VALUES (...)
                                     // and writes the generated Id back to task.Id

            return task;
        }

        /// <summary>
        /// Returns all non-archived tasks, ordered by column then position.
        /// LINQ: ctx.Tasks.Where(...).OrderBy(...) → SELECT ... WHERE IsArchived=0
        /// </summary>
        public List<KanbanTask> GetAllTasks()
        {
            using var ctx = new KanbanDbContext();
            return ctx.Tasks
                      .Where(t => !t.IsArchived)
                      .OrderBy(t => t.Column)
                      .ThenBy(t => t.Position)
                      .ToList();
        }

        /// <summary>Returns all archived tasks, newest first.</summary>
        public List<KanbanTask> GetArchivedTasks()
        {
            using var ctx = new KanbanDbContext();
            return ctx.Tasks
                      .Where(t => t.IsArchived)
                      .OrderByDescending(t => t.UpdatedAt)
                      .ToList();
        }

        /// <summary>Returns active tasks in a specific column.</summary>
        public List<KanbanTask> GetTasksByColumn(string column)
        {
            using var ctx = new KanbanDbContext();
            return ctx.Tasks
                      .Where(t => !t.IsArchived && t.Column == column)
                      .OrderBy(t => t.Position)
                      .ToList();
        }

        /// <summary>
        /// Saves changes to an existing task.
        /// Because the DbContext uses NoTracking by default, we use
        /// ctx.Update(task) to tell EF Core this is a modified entity
        /// (generates UPDATE ... WHERE Id = @id).
        /// </summary>
        public void UpdateTask(KanbanTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;

            using var ctx = new KanbanDbContext();
            ctx.Tasks.Update(task);  // EF emits: UPDATE Tasks SET ... WHERE Id = @id
            ctx.SaveChanges();
        }

        /// <summary>Marks every active task as archived (end-of-sprint).</summary>
        public void ArchiveAllTasks()
        {
            using var ctx = new KanbanDbContext();
            // ExecuteUpdate is an EF Core 7+ bulk-update API — no per-entity round trips
            ctx.Tasks
               .Where(t => !t.IsArchived)
               .ExecuteUpdate(s => s.SetProperty(t => t.IsArchived, true));
        }

        /// <summary>Restores an archived task back to the active board.</summary>
        public void RestoreTask(int taskId)
        {
            using var ctx = new KanbanDbContext();
            ctx.Tasks
               .Where(t => t.Id == taskId)
               .ExecuteUpdate(s => s.SetProperty(t => t.IsArchived, false));
        }

        /// <summary>Permanently deletes a single task by Id.</summary>
        public void DeleteTask(int taskId)
        {
            using var ctx = new KanbanDbContext();
            // ExecuteDelete is an EF Core 7+ bulk-delete API — no load-then-delete round trip
            ctx.Tasks
               .Where(t => t.Id == taskId)
               .ExecuteDelete();
        }

        /// <summary>Permanently deletes all archived tasks.</summary>
        public void DeleteAllArchived()
        {
            using var ctx = new KanbanDbContext();
            ctx.Tasks
               .Where(t => t.IsArchived)
               .ExecuteDelete();
        }
    }
}
