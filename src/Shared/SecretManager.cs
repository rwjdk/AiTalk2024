using Microsoft.Extensions.Configuration;

namespace Shared;



public record Secrets(
    string AzureOpenAiEndpoint,
    string AzureOpenAiApiKey,
    string CosmosDbConnectionString,
    string GoogleGeminiApiKey);

public class SecretManager
{
    //Right-click the Shared Project and choose "Manage User Secrets". Then provide the following:
    /*
    {
      "AzureOpenAiEndpoint": "todo",
      "AzureOpenAiKey": "todo",
      "CosmosDbConnectionString": "todo",
      "GoogleGeminiApiKey": "todo"
    }
    */

    private static readonly IConfigurationRoot Configuration =
        new ConfigurationBuilder()
            .AddUserSecrets<SecretManager>()
            .Build();

    public static Secrets GetSecrets()
    {
        return new Secrets(
            Configuration["AzureOpenAiEndpoint"]!,
            Configuration["AzureOpenAiKey"]!,
            Configuration["CosmosDbConnectionString"]!,
            Configuration["GoogleGeminiApiKey"]!
        );
    }
}