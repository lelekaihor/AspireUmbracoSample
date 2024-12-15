namespace AspireUmbracoSample.Aspire.Host;

public static class Constants
{
    public static class SqlServer
    {
        // Standard variables like SA password can be seen in aspire container details, we don't have to override or define them
        // if you need sql server to keep running when you stop app - change container lifetime to Persistent
        public const ContainerLifetime Lifetime = ContainerLifetime.Session;
        public const string ServiceName = "infrastructure-sqlserver";
        public const string ImageName = "mssql/server";
        //if you need to use another tag make sure to align entrypoint path to sql tools
        //which are set for current version => /opt/mssql-tools/bin/sqlcmd
        public const string ImageTag = "2022-CU13-ubuntu-22.04";
    }

    public static class BlobStorage
    {
        public const string ServiceName = "infrastructure-azurite";
        public const string SharedVolumeName = "azurite-data";

        public const string UmbracoBlobName = "media";
    }

    public static class Provisioning
    {
        public const string ServiceName = "app-provisioning";
    }

    public static class CmsWebApp
    {
        public const string ServiceName = "app-cms-web";

        public const string DatabaseFileName = "umbraco-dev-sql";
        public const string DatabaseConnectionStringName = "umbracoDbDSN";

        public static class UnattendedInstallation
        {
            public static readonly string Enabled = true.ToString();
            public const string UserName = "Admin User";
            public const string Email = "hello@sample.com";
            public const string Password = "LocalInstancePassword!";
        }
    }
}