using Microsoft.Extensions.Configuration;

namespace Shared;

public class SecretManager
{
    public static AzureOpenAiCredentials GetAzureOpenAiCredentials()
    {
        var configuration = GetConfiguration();
        return new AzureOpenAiCredentials(configuration["AzureOpenAiEndpoint"]!, configuration["AzureOpenAiKey"]!);
    }

    private static IConfigurationRoot GetConfiguration()
    {
        return new ConfigurationBuilder().AddUserSecrets<SecretManager>().Build();
    }
}