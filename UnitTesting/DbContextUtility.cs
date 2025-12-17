using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
public static class DbContextUtility
{
    public static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Creates a DbContext using SQLite in-memory database.
    /// SQLite enforces unique constraints, unlike the in-memory provider.
    /// Use this for tests that need to verify database constraint violations.
    /// Note: The connection is kept open and will be disposed when the context is disposed.
    /// </summary>
    public static AppDbContext CreateSqliteDbContext(string dbName)
    {
        // Use a unique in-memory database name for each test
        var connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        // Ensure the connection stays open by storing it (EF Core will dispose it when context is disposed)
        return context;
    }
}