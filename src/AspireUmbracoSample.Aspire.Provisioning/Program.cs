using AspireUmbracoSample.Aspire.Provisioning.Blob;
using AspireUmbracoSample.Aspire.Provisioning.ProvisioningConfiguration;
using AspireUmbracoSample.Aspire.Provisioning.Sql;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
//use to make sure DBs are available (re-attached or created)
    .AddHostedService<DatabasesAwaitingService>()
//use to make sure blob storage has public media folder
    .AddHostedService<BlobStorageInitializer<MediaBlobContainerProvisioningConfiguration>>();

var host = builder.Build();
await host.StartAsync();
await host.StopAsync();