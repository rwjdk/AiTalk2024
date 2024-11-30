using System.Text;
using Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using OpenAI.Chat;
using Shared;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0070

Secrets secrets = SecretManager.GetSecrets();

string azureOpenAiEndpoint = secrets.AzureOpenAiEndpoint;
string azureOpenAiApiKey = secrets.AzureOpenAiApiKey;
const string modelDeploymentName = "gpt-4o";

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(modelDeploymentName, azureOpenAiEndpoint, azureOpenAiApiKey);
Kernel kernel = builder.Build();

var agent = new ChatCompletionAgent
{
    Name = "MyAgent",
    Kernel = kernel,
    Instructions = "You are a nice AI Agent",
    Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    })
};

kernel.ImportPluginFromType<TimePlugin>();

var history = new ChatHistory();
history.AddUserMessage("The user is called Rasmus");

Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    Console.Write("> ");
    var question = Console.ReadLine() ?? "";
    history.AddUserMessage(question);

    await foreach (var response in agent.InvokeStreamingAsync(history))
    {
        Console.Write(response.Content ?? "");
        if (response.Metadata?.TryGetValue("Usage", out object? usage) == true)
        {
            if (usage is ChatTokenUsage chatTokenUsage)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine();
                Console.WriteLine($"[Token Usage for Request: {chatTokenUsage.InputTokenCount} In | {chatTokenUsage.OutputTokenCount} Out]");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }

    int totalInputTokens = 0;
    int totalOutputTokens = 0;
    foreach (ChatMessageContent historyEntry in history)
    {
        if (historyEntry.Metadata?.TryGetValue("Usage", out object? usage) == true)
        {
            if (usage is ChatTokenUsage chatTokenUsage)
            {
                totalInputTokens += chatTokenUsage.InputTokenCount;
                totalOutputTokens += chatTokenUsage.OutputTokenCount;
            }
        }
    }

    history.Clear();

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"[Additional Token Usage for Chat History: {totalInputTokens} In | {totalOutputTokens} Out]");
    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine();
    Console.WriteLine("*********************");
    Console.WriteLine();
}