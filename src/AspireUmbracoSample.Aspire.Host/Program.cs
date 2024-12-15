using AspireUmbracoSample.Aspire.Host.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer();
var mediaBlob = builder.AddBlobStorageEmulator();
var provisioningRunner = builder.AddProvisioningStartupRunner(sqlServer, mediaBlob, out var cmsDb);
var cmsWebApp = builder.AddCmsWebApp(mediaBlob, cmsDb, provisioningRunner);

builder.Build().Run();