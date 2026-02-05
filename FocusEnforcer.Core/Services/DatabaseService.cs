using System.Data;
using Dapper;
using FocusEnforcer.Core.Models;
using Microsoft.Data.Sqlite;

namespace FocusEnforcer.Core.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        // Common AppData folder for shared access between Service and UI
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FocusEnforcer");
        Directory.CreateDirectory(folder);
        string dbPath = Path.Combine(folder, "data.db");
        _connectionString = $"Data Source={dbPath}";
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = GetConnection();
        conn.Open();
        
        // BlockRules
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS BlockRules (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Type INTEGER NOT NULL,
                Behavior INTEGER NOT NULL DEFAULT 0,
                Value TEXT NOT NULL,
                IsEnabled INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL
            )");

        // Schema Migration: Check if Behavior column exists, if not add it
        try {
            conn.Execute("ALTER TABLE BlockRules ADD COLUMN Behavior INTEGER NOT NULL DEFAULT 0");
        } catch { /* Column likely exists */ }

        // AppConfig
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS AppConfig (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PasswordHash TEXT,
                IsFrozenTurkeyEnabled INTEGER,
                UnlockDifficultyLevel INTEGER,
                RequireRestartToUnlock INTEGER
            )");

        // Seed default config if empty
        var count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM AppConfig");
        if (count == 0)
        {
            conn.Execute("INSERT INTO AppConfig (PasswordHash, IsFrozenTurkeyEnabled, UnlockDifficultyLevel, RequireRestartToUnlock) VALUES ('', 0, 1, 0)");
        }
    }

    public IDbConnection GetConnection() => new SqliteConnection(_connectionString);

    // Basic CRUD examples
    public IEnumerable<BlockRule> GetActiveRules()
    {
        using var conn = GetConnection();
        return conn.Query<BlockRule>("SELECT * FROM BlockRules WHERE IsEnabled = 1");
    }

    public void AddRule(BlockRule rule)
    {
        using var conn = GetConnection();
        conn.Execute("INSERT INTO BlockRules (Type, Behavior, Value, IsEnabled, CreatedAt) VALUES (@Type, @Behavior, @Value, @IsEnabled, @CreatedAt)", rule);
    }
    
    public AppConfig GetConfig()
    {
        using var conn = GetConnection();
        return conn.QueryFirstOrDefault<AppConfig>("SELECT * FROM AppConfig LIMIT 1") ?? new AppConfig();
    }

    public void UpdateConfig(AppConfig config)
    {
         using var conn = GetConnection();
         conn.Execute("UPDATE AppConfig SET PasswordHash = @PasswordHash, IsFrozenTurkeyEnabled = @IsFrozenTurkeyEnabled WHERE Id = @Id", config);
    }

    public void DeleteRule(int id)
    {
        using var conn = GetConnection();
        conn.Execute("DELETE FROM BlockRules WHERE Id = @Id", new { Id = id });
    }
}
