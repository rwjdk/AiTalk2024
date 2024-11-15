using System.ClientModel;
using System.ComponentModel;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Shared;

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient azureOpenAiClient = new AzureOpenAIClient(
    new Uri(secrets.AzureOpenAiEndpoint),
    new ApiKeyCredential(secrets.AzureOpenAiApiKey));

[Description("Gets the current weather")]
string GetCurrentWeather() => Random.Shared.NextDouble() > 0.5 ? "It's sunny" : "It's raining";

IChatClient client = new ChatClientBuilder()
    .UseFunctionInvocation()
    .Use(azureOpenAiClient.AsChatClient(modelId: "gpt-4o-mini"));

Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    Console.Write("> ");
    var question = Console.ReadLine() ?? "";
    var response = client.CompleteStreamingAsync(
        question,
        new() { Tools = [AIFunctionFactory.Create(GetCurrentWeather)] });

    await foreach (var update in response)
    {
        Console.Write(update);
    }

    Console.WriteLine();
    Console.WriteLine("*********************");
    Console.WriteLine();
}