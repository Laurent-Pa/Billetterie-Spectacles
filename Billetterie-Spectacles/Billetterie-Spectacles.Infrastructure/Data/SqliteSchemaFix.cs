using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Billetterie_Spectacles.Infrastructure.Data;

/// <summary>
/// Corrige le schéma SQLite après application des migrations créées pour SQL Server :
/// les colonnes identity doivent être INTEGER PRIMARY KEY AUTOINCREMENT pour que l'INSERT sans valeur fonctionne.
/// </summary>
public static class SqliteSchemaFix
{
    public static void ApplyIfNeeded(string connectionString, bool useSqlite)
    {
        if (!useSqlite || string.IsNullOrWhiteSpace(connectionString))
            return;

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var usersOk = IsUsersTableAutoIncrement(connection);
        var ordersHasPaymentIntent = HasColumn(connection, "Orders", "payment_intent_id");

        if (usersOk && ordersHasPaymentIntent)
        {
            Console.WriteLine("Schéma SQLite OK (Users + Orders).");
            return;
        }

        Console.WriteLine("Correction du schéma SQLite (AUTOINCREMENT sur les clés)...");

        using var trans = connection.BeginTransaction();
        try
        {
            ExecuteNonQuery(connection, trans, "PRAGMA foreign_keys = OFF;");

            // Users
            RecreateTableWithAutoIncrement(connection, trans, "Users",
                "user_id INTEGER PRIMARY KEY AUTOINCREMENT",
                "name TEXT NOT NULL", "surname TEXT NOT NULL", "email TEXT NOT NULL",
                "password TEXT NOT NULL", "phone TEXT", "role INTEGER NOT NULL",
                "created_at TEXT NOT NULL", "updated_at TEXT NOT NULL");
            CreateIndexIfNotExists(connection, trans, "IX_Users_email", "Users", "email", unique: true);

            // Spectacles
            RecreateTableWithAutoIncrement(connection, trans, "Spectacles",
                "spectacle_id INTEGER PRIMARY KEY AUTOINCREMENT",
                "name TEXT NOT NULL", "category INTEGER NOT NULL", "description TEXT",
                "duration INTEGER NOT NULL", "thumbnail TEXT", "created_at TEXT NOT NULL",
                "updated_at TEXT NOT NULL", "created_by_user_id INTEGER NOT NULL");
            CreateIndexIfNotExists(connection, trans, "IX_Spectacles_created_by_user_id", "Spectacles", "created_by_user_id", unique: false);

            // Performances
            RecreateTableWithAutoIncrement(connection, trans, "Performances",
                "performance_id INTEGER PRIMARY KEY AUTOINCREMENT",
                "date TEXT NOT NULL", "status INTEGER NOT NULL", "capacity INTEGER NOT NULL",
                "available_tickets INTEGER NOT NULL", "created_at TEXT NOT NULL", "updated_at TEXT NOT NULL",
                "spectacle_id INTEGER NOT NULL", "unit_price REAL NOT NULL");
            CreateIndexIfNotExists(connection, trans, "IX_Performances_spectacle_id", "Performances", "spectacle_id", unique: false);

            // Orders
            RecreateTableWithAutoIncrement(connection, trans, "Orders",
                "order_id INTEGER PRIMARY KEY AUTOINCREMENT",
                "status INTEGER NOT NULL", "total_price REAL NOT NULL", "created_at TEXT NOT NULL",
                "updated_at TEXT NOT NULL", "user_id INTEGER NOT NULL", "payment_intent_id TEXT");
            CreateIndexIfNotExists(connection, trans, "IX_Orders_user_id", "Orders", "user_id", unique: false);

            // Tickets
            RecreateTableWithAutoIncrement(connection, trans, "Tickets",
                "ticket_id INTEGER PRIMARY KEY AUTOINCREMENT",
                "status INTEGER NOT NULL", "unit_price REAL NOT NULL", "order_id INTEGER NOT NULL",
                "performance_id INTEGER NOT NULL", "created_at TEXT NOT NULL", "updated_at TEXT NOT NULL");
            CreateIndexIfNotExists(connection, trans, "IX_Tickets_order_id", "Tickets", "order_id", unique: false);
            CreateIndexIfNotExists(connection, trans, "IX_Tickets_performance_id", "Tickets", "performance_id", unique: false);

            ExecuteNonQuery(connection, trans, "PRAGMA foreign_keys = ON;");
            trans.Commit();
            Console.WriteLine("Schéma SQLite corrigé.");
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    private static void RecreateTableWithAutoIncrement(SqliteConnection connection, SqliteTransaction trans,
        string tableName, string pkColumn, params string[] columns)
    {
        var allColumns = new[] { pkColumn }.Concat(columns).ToArray();
        var columnsList = string.Join(", ", allColumns);
        var tableNew = tableName + "_new";

        // Vérifier si la table existe (migration peut avoir créé des noms différents)
        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = trans;
            cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            if (cmd.ExecuteScalar() == null)
                return; // Table pas encore créée par les migrations
        }

        var existingColumns = GetColumnNames(connection, trans, tableName);
        if (existingColumns.Count == 0) return;

        var insertCols = string.Join(", ", existingColumns.Select(c => "\"" + c + "\""));
        var selectCols = string.Join(", ", existingColumns.Select(c => "\"" + c + "\""));

        ExecuteNonQuery(connection, trans, $"CREATE TABLE \"{tableNew}\" ({columnsList});");
        ExecuteNonQuery(connection, trans, $"INSERT INTO \"{tableNew}\" ({insertCols}) SELECT {selectCols} FROM \"{tableName}\";");
        ExecuteNonQuery(connection, trans, $"DROP TABLE \"{tableName}\";");
        ExecuteNonQuery(connection, trans, $"ALTER TABLE \"{tableNew}\" RENAME TO \"{tableName}\";");
    }

    private static bool IsUsersTableAutoIncrement(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='Users'";
        var sql = cmd.ExecuteScalar() as string;
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        var normalized = sql.Replace("\"", "").ToLowerInvariant();
        if (normalized.Contains("autoincrement"))
            return true;

        return normalized.Contains("user_id integer primary key");
    }

    private static bool HasColumn(SqliteConnection connection, string tableName, string columnName)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{tableName}\")";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            if (string.Equals(r.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static List<string> GetColumnNames(SqliteConnection connection, SqliteTransaction trans, string tableName)
    {
        var list = new List<string>();
        using var cmd = connection.CreateCommand();
        cmd.Transaction = trans;
        cmd.CommandText = $"PRAGMA table_info(\"{tableName}\")";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(r.GetString(1)); // name
        return list;
    }

    private static void CreateIndexIfNotExists(SqliteConnection connection, SqliteTransaction trans,
        string indexName, string tableName, string columnName, bool unique)
    {
        var uniqueStr = unique ? "UNIQUE " : "";
        try
        {
            ExecuteNonQuery(connection, trans,
                $"CREATE {uniqueStr}INDEX IF NOT EXISTS \"{indexName}\" ON \"{tableName}\" (\"{columnName}\");");
        }
        catch { /* index peut déjà exister */ }
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction trans, string sql)
    {
        foreach (var stmt in sql.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var s = stmt.Trim();
            if (s.Length == 0) continue;
            using var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = s;
            cmd.ExecuteNonQuery();
        }
    }
}
