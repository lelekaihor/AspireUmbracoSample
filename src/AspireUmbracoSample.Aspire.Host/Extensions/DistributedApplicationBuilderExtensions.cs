using Aspire.Hosting.Azure;
using Projects;

namespace AspireUmbracoSample.Aspire.Host.Extensions;

public static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<SqlServerServerResource> AddSqlServer(this IDistributedApplicationBuilder distributedApplicationBuilder)
    {
        return distributedApplicationBuilder
            .AddSqlServer(Constants.SqlServer.ServiceName)
            .WithImage(image: Constants.SqlServer.ImageName, tag: Constants.SqlServer.ImageTag)
            .WithEnvironment("MSSQL_BACKUP_DIR", "/var/opt/mssql")
            .WithEnvironment("MSSQL_DATA_DIR", "/var/opt/data")
            .WithEnvironment("MSSQL_BACKUP_DIR", "/var/opt/data")
            .WithBindMount("./aspire/sql/data", "/var/opt/data")
            .WithLifetime(Constants.SqlServer.Lifetime);
    }

    public static IResourceBuilder<AzureBlobStorageResource> AddBlobStorageEmulator(this IDistributedApplicationBuilder builder)
    {
        var storage = builder.AddAzureStorage(Constants.BlobStorage.ServiceName);
        storage.RunAsEmulator(azurite => azurite.WithDataVolume(Constants.BlobStorage.SharedVolumeName));
        return storage.AddBlobs(Constants.BlobStorage.UmbracoBlobName);
    }

    public static IResourceBuilder<ProjectResource> AddProvisioningStartupRunner(this IDistributedApplicationBuilder distributedApplicationBuilder,
        IResourceBuilder<SqlServerServerResource> sqlServer,
        IResourceBuilder<AzureBlobStorageResource> mediaBlob,
        out IResourceBuilder<SqlServerDatabaseResource> cmsDb)
    {
        HashSet<string> databasesForAutoCreation = [Constants.CmsWebApp.DatabaseFileName];

        sqlServer
            .WithEnvironment("AUTO_CREATION_DATABASE_NAMES", string.Join(',', databasesForAutoCreation))
            .WithBindMount("./aspire/sql/entrypoint.sh", "/usr/local/bin/entrypoint.sh")
            .WithEntrypoint("/usr/local/bin/entrypoint.sh");

        cmsDb = sqlServer.AddDatabase(Constants.CmsWebApp.DatabaseConnectionStringName, Constants.CmsWebApp.DatabaseFileName);

        HashSet<string> databasesToAwait = [Constants.CmsWebApp.DatabaseConnectionStringName];

        return distributedApplicationBuilder
            .AddProject<AspireUmbracoSample_Aspire_Provisioning>(Constants.Provisioning.ServiceName)
            .WithEnvironment("AWAIT_DATABASES", string.Join(',', databasesToAwait))
            .WithReference(mediaBlob)
            .WaitFor(mediaBlob)
            .WithReference(cmsDb)
            .WaitFor(cmsDb);
    }

    public static IResourceBuilder<ProjectResource> AddCmsWebApp(this IDistributedApplicationBuilder builder,
        IResourceBuilder<AzureBlobStorageResource> mediaBlob,
        IResourceBuilder<SqlServerDatabaseResource> cmsDb,
        IResourceBuilder<ProjectResource> provisioningStartupRunner)
    {
        return builder
            .AddProject<AspireUmbracoSample_Web>(Constants.CmsWebApp.ServiceName)
            .WithReference(mediaBlob)
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables["Umbraco__Storage__AzureBlob__Media__ConnectionString"] = new ConnectionStringReference(mediaBlob.Resource, false);
            })
            .WithEnvironment("Umbraco__Storage__AzureBlob__Media__ContainerName", mediaBlob.Resource.Name)
            .WithReference(cmsDb)
            .WithEnvironment("umbracoDbDSN_ProviderName", "System.Data.SqlClient")
            .WithEnvironment("Umbraco__CMS__Unattended__InstallUnattended", Constants.CmsWebApp.UnattendedInstallation.Enabled)
            .WithEnvironment("Umbraco__CMS__Unattended__UnattendedUserName", Constants.CmsWebApp.UnattendedInstallation.UserName)
            .WithEnvironment("Umbraco__CMS__Unattended__UnattendedUserEmail", Constants.CmsWebApp.UnattendedInstallation.Email)
            .WithEnvironment("Umbraco__CMS__Unattended__UnattendedUserPassword", Constants.CmsWebApp.UnattendedInstallation.Password)
            .WaitForCompletion(provisioningStartupRunner);
    }
}