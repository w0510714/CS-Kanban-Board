using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.Data
{
    // Manages all SQLite database operations
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            // Set database path to the execution directory
            string dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "kanban.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        // Creates tables and applies necessary schema updates
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    [Column] TEXT NOT NULL,
                    Position INTEGER NOT NULL,
                    IsArchived INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    Priority TEXT NOT NULL DEFAULT 'Medium'
                );
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );", connection);
            cmd.ExecuteNonQuery();

            // Schema migrations for older databases
            try { using var migrateCmd = new SqliteCommand("ALTER TABLE Tasks ADD COLUMN IsArchived INTEGER NOT NULL DEFAULT 0;", connection); migrateCmd.ExecuteNonQuery(); } catch { }
            try { using var migrateCmd = new SqliteCommand("ALTER TABLE Tasks ADD COLUMN Priority TEXT NOT NULL DEFAULT 'Medium';", connection); migrateCmd.ExecuteNonQuery(); } catch { }
        }

        public string GetSetting(string key, string defaultValue)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand("SELECT Value FROM Settings WHERE Key = @key", connection);
            cmd.Parameters.AddWithValue("@key", key);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? defaultValue;
        }

        public void SaveSetting(string key, string value)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = new SqliteCommand(@"
                INSERT INTO Settings (Key, Value) VALUES (@key, @val)
                ON CONFLICT(Key) DO UPDATE SET Value = @val", connection);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@val", value);
            cmd.ExecuteNonQuery();
        }

        // Persist a new task to the database
        public KanbanTask AddTask(KanbanTask task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                INSERT INTO Tasks (Title, Description, Priority, Column, Position, IsArchived, CreatedAt, UpdatedAt)
                VALUES (@Title, @Description, @Priority, @Column, @Position, @IsArchived, @CreatedAt, @UpdatedAt);
                SELECT last_insert_rowid();
                """;

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Description", task.Description);
            cmd.Parameters.AddWithValue("@Priority", task.Priority);
            cmd.Parameters.AddWithValue("@Column", task.Column);
            cmd.Parameters.AddWithValue("@Position", task.Position);
            cmd.Parameters.AddWithValue("@IsArchived", task.IsArchived ? 1 : 0);
            cmd.Parameters.AddWithValue("@CreatedAt", task.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("@UpdatedAt", task.UpdatedAt.ToString("o"));

            var result = cmd.ExecuteScalar();
            task.Id = Convert.ToInt32(result);

            return task;
        }

        // Load all non-archived tasks
        public List<KanbanTask> GetAllTasks()
        {
            var tasks = new List<KanbanTask>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                SELECT Id, Title, Description, Column, Position, IsArchived, CreatedAt, UpdatedAt, Priority
                FROM   Tasks
                WHERE  IsArchived = 0
                ORDER  BY Column, Position;
                """;

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapRow(reader));
            }

            return tasks;
        }

        // Load all archived tasks
        public List<KanbanTask> GetArchivedTasks()
        {
            var tasks = new List<KanbanTask>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                SELECT Id, Title, Description, Column, Position, IsArchived, CreatedAt, UpdatedAt, Priority
                FROM   Tasks
                WHERE  IsArchived = 1
                ORDER  BY UpdatedAt DESC;
                """;

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapRow(reader));
            }

            return tasks;
        }

        // Load active tasks for a specific column
        public List<KanbanTask> GetTasksByColumn(string column)
        {
            var tasks = new List<KanbanTask>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                SELECT Id, Title, Description, Column, Position, IsArchived, CreatedAt, UpdatedAt, Priority
                FROM   Tasks
                WHERE  IsArchived = 0 AND Column = @Column
                ORDER  BY Position;
                """;

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Column", column);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(MapRow(reader));
            }

            return tasks;
        }

        // Update an existing task's data
        public void UpdateTask(KanbanTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                UPDATE Tasks
                SET    Title       = @Title,
                       Description = @Description,
                       Priority    = @Priority,
                       Column      = @Column,
                       Position    = @Position,
                       IsArchived  = @IsArchived,
                       UpdatedAt   = @UpdatedAt
                WHERE  Id = @Id;
                """;

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Description", task.Description);
            cmd.Parameters.AddWithValue("@Priority", task.Priority);
            cmd.Parameters.AddWithValue("@Column", task.Column);
            cmd.Parameters.AddWithValue("@Position", task.Position);
            cmd.Parameters.AddWithValue("@IsArchived", task.IsArchived ? 1 : 0);
            cmd.Parameters.AddWithValue("@UpdatedAt", task.UpdatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("@Id", task.Id);

            cmd.ExecuteNonQuery();
        }

        // Clear the board by archiving all active tasks
        public void ArchiveAllTasks()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = "UPDATE Tasks SET IsArchived = 1 WHERE IsArchived = 0;";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        public void RestoreTask(int taskId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = "UPDATE Tasks SET IsArchived = 0 WHERE Id = @Id;";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", taskId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteTask(int taskId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = "DELETE FROM Tasks WHERE Id = @Id;";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", taskId);
            cmd.ExecuteNonQuery();
        }

        // Permanently delete everything in the archive
        public void DeleteAllArchived()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = "DELETE FROM Tasks WHERE IsArchived = 1;";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        // Helper to map a database row to a KanbanTask object
        private static KanbanTask MapRow(SqliteDataReader reader)
        {
            return new KanbanTask
            {
                Id          = reader.GetInt32(0),
                Title       = reader.IsDBNull(1) ? "Untitled" : reader.GetString(1),
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Column      = reader.IsDBNull(3) ? "To Do" : reader.GetString(3),
                Position    = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                IsArchived  = reader.IsDBNull(5) ? false : reader.GetInt32(5) == 1,
                CreatedAt   = reader.IsDBNull(6) ? DateTime.UtcNow : DateTime.Parse(reader.GetString(6)),
                UpdatedAt   = reader.IsDBNull(7) ? DateTime.UtcNow : DateTime.Parse(reader.GetString(7)),
                Priority    = reader.IsDBNull(8) ? "Medium" : reader.GetString(8)
            };
        }
    }
}
