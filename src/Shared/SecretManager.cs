using Microsoft.Extensions.Configuration;

namespace Shared;

public record Secrets(
    string AzureOpenAiEndpoint,
    string AzureOpenAiApiKey,
    string AzureSpeechApiKey,
    string CosmosDbConnectionString,
    string GoogleGeminiApiKey);

public class SecretManager
{
    private static readonly IConfigurationRoot Configuration =
        new ConfigurationBuilder()
            .AddUserSecrets<SecretManager>()
            .Build();

    public static Secrets GetSecrets()
    {
        return new Secrets(
            Configuration["AzureOpenAiEndpoint"]!,
            Configuration["AzureOpenAiKey"]!,
            Configuration["AzureSpeechKey"]!,
            Configuration["CosmosDbConnectionString"]!,
            Configuration["GoogleGeminiApiKey"]!
        );
    }
}