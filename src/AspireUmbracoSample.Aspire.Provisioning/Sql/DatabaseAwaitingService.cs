using Microsoft.Data.SqlClient;

namespace AspireUmbracoSample.Aspire.Provisioning.Sql;

public class DatabasesAwaitingService(IConfiguration configuration) : IHostedService
{
    private readonly HashSet<string> _databasesToAwait = configuration
        .GetValue<string>("AWAIT_DATABASES")?
        .Split(',')
        .ToHashSet() ?? [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Task.WaitAll(_databasesToAwait
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(databaseToAwait => WaitForDatabaseAsync(connectionStringName: databaseToAwait, maxAttempts: 100, delayMilliseconds: 1000))
            .ToArray(), cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    private async Task WaitForDatabaseAsync(string connectionStringName, int maxAttempts, int delayMilliseconds)
    {
        Console.WriteLine($"Checking {connectionStringName} database availability...");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (await IsDatabaseAvailableAsync(connectionStringName))
            {
                Console.WriteLine($"Database {connectionStringName} is available.");
                return;
            }

            Console.WriteLine($"{connectionStringName}: Attempt {attempt} failed. Retrying in {delayMilliseconds}ms...");
            await Task.Delay(delayMilliseconds);
        }

        throw new Exception($"Database {connectionStringName} did not become available within the maximum number of attempts.");
    }

    private async Task<bool> IsDatabaseAvailableAsync(string connectionStringName)
    {
        try
        {
            await using var connection = new SqlConnection(configuration.GetConnectionString(connectionStringName));
            await connection.OpenAsync();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }
}