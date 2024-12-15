namespace AspireUmbracoSample.Aspire.Provisioning.ProvisioningConfiguration;

public interface IBlobContainerInitSettings : IConnectionStringNameProvider
{
    public static abstract string ContainerName { get; }
}