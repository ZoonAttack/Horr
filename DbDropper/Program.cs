using System;
using Microsoft.Data.SqlClient;

string connectionString = "Server=db56850.databaseasp.net; Database=db56850; User Id=db56850; Password=w%7NZn!8?F4p; Encrypt=False; MultipleActiveResultSets=True;";

string dropScript = @"
-- Drop foreign keys
DECLARE @Sql NVARCHAR(MAX) = '';
SELECT @Sql += 'ALTER TABLE ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(object_name(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys;
EXEC sp_executesql @Sql;

-- Drop tables
SET @Sql = '';
SELECT @Sql += 'DROP TABLE ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.tables;
EXEC sp_executesql @Sql;
";

using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();
    using (SqlCommand command = new SqlCommand(dropScript, connection))
    {
        command.ExecuteNonQuery();
    }
}
Console.WriteLine("All tables dropped successfully.");
