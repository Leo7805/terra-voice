# SQLite Template (Minimal)

## Purpose

Lightweight SQLite setup for simple scenarios (e.g., usage tracking, counters, small tools).

---

## 1. Install

```bash
dotnet add package Microsoft.Data.Sqlite
```

---

## 2. Create Database (Ensure Directory + Open Connection)

```csharp
// Ensure directory exists (SQLite will NOT create directories)
Directory.CreateDirectory("Data");

// Create connection (does NOT open yet)
await using var connection = new SqliteConnection("Data Source=Data/usage.db");

// Open connection (required before executing any SQL)
await connection.OpenAsync();
```

---

## 3. Create Table (Idempotent Initialization)

```csharp
var command = connection.CreateCommand();

command.CommandText = """
CREATE TABLE IF NOT EXISTS usage_monthly (
    month TEXT PRIMARY KEY,
    request_count INTEGER NOT NULL DEFAULT 0,
    char_count INTEGER NOT NULL DEFAULT 0,
    updated_at TEXT NOT NULL
);
""";

await command.ExecuteNonQueryAsync();
```

## 4. Create DB when program launched

- in Program.cs

```cs
// var app = builder.Build();   // add after this line
await UsageDb.InitializeAsync();
```

---

## 4. Insert / Update (Upsert Pattern)

```csharp
var command = connection.CreateCommand();

command.CommandText = """
INSERT INTO usage_monthly (month, request_count, char_count, updated_at)
VALUES ($month, 1, $charCount, $updatedAt)
ON CONFLICT(month) DO UPDATE SET
    request_count = request_count + 1,
    char_count = char_count + $charCount,
    updated_at = $updatedAt;
""";

command.Parameters.AddWithValue("$month", "2026-05");
command.Parameters.AddWithValue("$charCount", 120);
command.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("O"));

await command.ExecuteNonQueryAsync();
```

---

## 5. Query Data (Read)

```csharp
var command = connection.CreateCommand();

command.CommandText = """
SELECT month, request_count, char_count, updated_at
FROM usage_monthly
ORDER BY month DESC;
""";

await using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    var month = reader.GetString(0);
    var requestCount = reader.GetInt32(1);
    var charCount = reader.GetInt32(2);
    var updatedAt = reader.GetString(3);

    Console.WriteLine($"{month}: {requestCount} requests, {charCount} chars");
}
```

---

## 6. Key Notes

```txt
✔ SQLite creates the database file automatically if it does not exist
✔ Directory must exist BEFORE opening connection
✔ Use async APIs in web applications (OpenAsync, ExecuteAsync)
✔ CREATE TABLE IF NOT EXISTS is safe to run multiple times
✔ Use parameterized queries to avoid SQL injection
✔ await using ensures connection is properly disposed
```

---

## 7. When to Use This Pattern

```txt
✔ Usage tracking / counters
✔ Small internal tools
✔ Single-table or low-complexity data
✔ MVP / prototype systems
```

---

## 8. When NOT to Use SQLite

```txt
❌ High concurrency systems
❌ Complex relational data (many tables, joins)
❌ Large-scale production systems
❌ When you need advanced querying or scaling
```

---

## 9. Migration Path (Future)

```txt
SQLite → SQL Server / PostgreSQL
Raw SQL → EF Core (if complexity grows)
```
