using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AspireUmbracoSample.Aspire.Provisioning.ProvisioningConfiguration;

namespace AspireUmbracoSample.Aspire.Provisioning.Blob;

public class BlobStorageInitializer<TBlobInitSettings>
    : IHostedService where TBlobInitSettings : IBlobContainerInitSettings, new()
{
    private readonly string _containerName;
    private readonly string _connectionString;

    public BlobStorageInitializer(IConfiguration configuration)
    {
        var connectionStringName = TBlobInitSettings.ConnectionStringName;
        _containerName = TBlobInitSettings.ContainerName;
        _connectionString = configuration.GetConnectionString(connectionStringName)
                            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Checking Azure Blob Storage availability...");

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        if (await blobContainerClient.ExistsAsync(cancellationToken))
        {
            Console.WriteLine($"Blob container '{_containerName}' is available.");
            return;
        }

        Console.WriteLine($"Container '{_containerName}' does not exist. Attempting to create...");
        await blobContainerClient.CreateAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        Console.WriteLine($"Blob container '{_containerName}' created successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // No special cleanup is needed for this service.
        return Task.CompletedTask;
    }
}