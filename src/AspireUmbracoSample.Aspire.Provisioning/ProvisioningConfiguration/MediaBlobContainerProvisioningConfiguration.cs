namespace AspireUmbracoSample.Aspire.Provisioning.ProvisioningConfiguration;

public class MediaBlobContainerProvisioningConfiguration : IBlobContainerInitSettings
{
    public static string ConnectionStringName => "media";

    public static string ContainerName => "media";
}