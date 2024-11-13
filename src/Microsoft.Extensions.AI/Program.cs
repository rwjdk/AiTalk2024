using System.ClientModel;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Shared;

var azureOpenAiCredentials = SecretManager.GetAzureOpenAiCredentials();

AzureOpenAIClient azureOpenAiClient = new AzureOpenAIClient(
    new Uri(azureOpenAiCredentials.Endpoint),
    new ApiKeyCredential(azureOpenAiCredentials.ApiKey));

IChatClient client = azureOpenAiClient.AsChatClient(modelId: "gpt-4o-mini");
Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    Console.Write("Question: ");
    var question = Console.ReadLine() ?? "";
    await foreach (var update in client.CompleteStreamingAsync(question))
    {
        Console.Write(update.Text);
    }

    Console.WriteLine();
    Console.WriteLine("*********************");
    Console.WriteLine();
}