namespace AspireUmbracoSample.Aspire.Provisioning.ProvisioningConfiguration;

public interface IConnectionStringNameProvider
{
    public static abstract string ConnectionStringName { get; }
}