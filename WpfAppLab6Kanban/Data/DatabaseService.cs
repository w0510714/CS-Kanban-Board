using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.Data
{
    // Wraps EF Core operations using a short-lived DbContext per call (unit-of-work pattern).
    // EF Core handles connection lifecycle, parameter binding, and row mapping automatically.
    public class DatabaseService
    {
        public DatabaseService()
        {
            // Create the DB file and tables if they don't yet exist.
            using var ctx = new KanbanDbContext();
            ctx.Database.EnsureCreated();
        }

        // ── Settings ──────────────────────────────────────────────────────

        /// <summary>Returns a persisted setting value, or defaultValue if not found.</summary>
        public string GetSetting(string key, string defaultValue)
        {
            using var ctx = new KanbanDbContext();
            return ctx.Settings.FirstOrDefault(s => s.Key == key)?.Value ?? defaultValue;
        }

        /// <summary>Upserts (insert or update) a setting value.</summary>
        public void SaveSetting(string key, string value)
        {
            using var ctx = new KanbanDbContext();
            var existing = ctx.Settings.AsTracking().FirstOrDefault(s => s.Key == key);

            if (existing is null)
                ctx.Settings.Add(new Setting { Key = key, Value = value });
            else
                existing.Value = value;

            ctx.SaveChanges();
        }

        // ── Task CRUD ─────────────────────────────────────────────────────

        /// <summary>Inserts a new task; EF Core sets task.Id after SaveChanges.</summary>
        public KanbanTask AddTask(KanbanTask task)
        {
            task.CreatedAt = task.UpdatedAt = DateTime.UtcNow;
            using var ctx = new KanbanDbContext();
            ctx.Tasks.Add(task);
            ctx.SaveChanges();
            return task;
        }

        /// <summary>Returns all non-archived tasks, ordered by column then position.</summary>
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

        /// <summary>Saves changes to an existing task.</summary>
        public void UpdateTask(KanbanTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            using var ctx = new KanbanDbContext();
            ctx.Tasks.Update(task);
            ctx.SaveChanges();
        }

        /// <summary>Marks every active task as archived (end-of-sprint).</summary>
        public void ArchiveAllTasks()
        {
            using var ctx = new KanbanDbContext();
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
