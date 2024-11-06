using Microsoft.Extensions.Configuration;

namespace Shared;

public class SecretManager
{
    public static AzureOpenAiCredentials GetAzureOpenAiCredentials()
    {
        var configuration = GetConfiguration();
        return new AzureOpenAiCredentials(configuration["AzureOpenAiEndpoint"]!, configuration["AzureOpenAiKey"]!);
    }

    public static TrelloCredentials GetTrelloCredentials()
    {
        var configuration = GetConfiguration();
        return new TrelloCredentials(configuration["TrelloApiKey"]!, configuration["TrelloToken"]!);
    }

    public static string GetCosmosDbConnectionString()
    {
        return GetConfiguration()["CosmosDbConnectionString"]!;
    }

    public static string GetAzureSpeechKey()
    {
        return GetConfiguration()["AzureSpeechKey"]!;
    }

    private static IConfigurationRoot GetConfiguration()
    {
        return new ConfigurationBuilder().AddUserSecrets<SecretManager>().Build();
    }
}