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

    public static AppDbContext CreateSqliteDbContext(string dbName)
    {
        var connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        return new AppDbContext(options);
    }
}
