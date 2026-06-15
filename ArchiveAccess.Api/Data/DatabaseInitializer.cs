namespace ArchiveAccess.Api.Data;

public sealed class DatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        SqliteConnectionFactory connectionFactory,
        ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public void Initialize()
    {
        ExecuteScript("Scripts/001_schema.sql");
        ExecuteScript("Scripts/002_seed.sql");

        Directory.CreateDirectory("Files");
        _logger.LogInformation("Database schema and initial data were prepared.");
    }

    private void ExecuteScript(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"SQL script not found: {path}");
        }

        var script = File.ReadAllText(path);

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = script;
        command.ExecuteNonQuery();
    }
}