using Microsoft.Data.Sqlite;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.Data
{
    /// <summary>
    /// Handles all SQLite database interactions for the Kanban board.
    ///
    /// Responsibilities:
    ///   - Create the database file and tables on first launch
    ///   - Provide full CRUD (Create, Read, Update, Delete) for KanbanTask
    ///
    /// The database file "kanban.db" is stored next to the running .exe so it
    /// persists between app launches without any extra configuration.
    /// </summary>
    public class DatabaseService
    {
        // ── Connection string ────────────────────────────────────────────────────
        // AppContext.BaseDirectory → the folder that contains the built .exe
        private readonly string _connectionString;

        public DatabaseService()
        {
            string dbPath = System.IO.Path.Combine(
                AppContext.BaseDirectory, "kanban.db");

            _connectionString = $"Data Source={dbPath}";

            // Always make sure the database and tables exist before any query runs.
            InitializeDatabase();
        }

        // ── Schema Bootstrap ─────────────────────────────────────────────────────

        /// <summary>
        /// Creates the Tasks table if it does not already exist.
        /// "IF NOT EXISTS" makes this safe to call on every startup.
        /// </summary>
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string createTableSql = """
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title       TEXT    NOT NULL,
                    Description TEXT    NOT NULL DEFAULT '',
                    Column      TEXT    NOT NULL DEFAULT 'To Do',
                    Position    INTEGER NOT NULL DEFAULT 0,
                    IsArchived  INTEGER NOT NULL DEFAULT 0,
                    CreatedAt   TEXT    NOT NULL,
                    UpdatedAt   TEXT    NOT NULL
                );
                """;

            using var cmd = new SqliteCommand(createTableSql, connection);
            cmd.ExecuteNonQuery();

            // Migration: Add IsArchived if it doesn't exist (handle existing DB from previous runs)
            try
            {
                using var migrateCmd = new SqliteCommand("ALTER TABLE Tasks ADD COLUMN IsArchived INTEGER NOT NULL DEFAULT 0;", connection);
                migrateCmd.ExecuteNonQuery();
            }
            catch { /* Column already exists, ignore error */ }
        }

        // ── CREATE ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Inserts a new task into the database and sets its generated Id.
        /// Returns the same task object so callers can chain if needed.
        /// </summary>
        public KanbanTask AddTask(KanbanTask task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                INSERT INTO Tasks (Title, Description, Column, Position, IsArchived, CreatedAt, UpdatedAt)
                VALUES (@Title, @Description, @Column, @Position, @IsArchived, @CreatedAt, @UpdatedAt);
                SELECT last_insert_rowid();
                """;

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Title",       task.Title);
            cmd.Parameters.AddWithValue("@Description", task.Description);
            cmd.Parameters.AddWithValue("@Column",      task.Column);
            cmd.Parameters.AddWithValue("@Position",    task.Position);
            cmd.Parameters.AddWithValue("@IsArchived",  task.IsArchived ? 1 : 0);
            cmd.Parameters.AddWithValue("@CreatedAt",   task.CreatedAt.ToString("o")); // ISO-8601
            cmd.Parameters.AddWithValue("@UpdatedAt",   task.UpdatedAt.ToString("o"));

            // last_insert_rowid() returns the auto-generated primary key
            var result = cmd.ExecuteScalar();
            task.Id = Convert.ToInt32(result);

            return task;
        }

        // ── READ ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns every task in the database, ordered by Column then Position.
        /// </summary>
        public List<KanbanTask> GetAllTasks()
        {
            var tasks = new List<KanbanTask>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                SELECT Id, Title, Description, Column, Position, IsArchived, CreatedAt, UpdatedAt
                FROM   Tasks
                WHERE  IsArchived = 0
                ORDER  BY Column, Position;
                """;

            using var cmd    = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapRow(reader));
            }

            return tasks;
        }

        /// <summary>
        /// Returns all tasks that belong to a specific column.
        /// </summary>
        public List<KanbanTask> GetTasksByColumn(string column)
        {
            var tasks = new List<KanbanTask>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                SELECT Id, Title, Description, Column, Position, IsArchived, CreatedAt, UpdatedAt
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

        // ── UPDATE ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Saves changes to an existing task back to the database.
        /// Always refreshes UpdatedAt to the current UTC time.
        /// </summary>
        public void UpdateTask(KanbanTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                UPDATE Tasks
                SET    Title       = @Title,
                       Description = @Description,
                       Column      = @Column,
                       Position    = @Position,
                       IsArchived  = @IsArchived,
                       UpdatedAt   = @UpdatedAt
                WHERE  Id = @Id;
                """;

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Title",       task.Title);
            cmd.Parameters.AddWithValue("@Description", task.Description);
            cmd.Parameters.AddWithValue("@Column",      task.Column);
            cmd.Parameters.AddWithValue("@Position",    task.Position);
            cmd.Parameters.AddWithValue("@IsArchived",  task.IsArchived ? 1 : 0);
            cmd.Parameters.AddWithValue("@UpdatedAt",   task.UpdatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("@Id",          task.Id);

            cmd.ExecuteNonQuery();
        }

        // ── DELETE ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Permanently removes a task from the database by its Id.
        /// </summary>
        public void DeleteTask(int taskId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = "DELETE FROM Tasks WHERE Id = @Id;";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", taskId);
            cmd.ExecuteNonQuery();
        }

        // ── Helper ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a single database row to a KanbanTask object.
        /// Centralised here so every Read method uses identical mapping logic.
        /// </summary>
        private static KanbanTask MapRow(SqliteDataReader reader)
        {
            return new KanbanTask
            {
                Id          = reader.GetInt32(0),
                Title       = reader.GetString(1),
                Description = reader.GetString(2),
                Column      = reader.GetString(3),
                Position    = reader.GetInt32(4),
                IsArchived  = reader.GetInt32(5) == 1,
                CreatedAt   = DateTime.Parse(reader.GetString(6)),
                UpdatedAt   = DateTime.Parse(reader.GetString(7))
            };
        }
    }
}
