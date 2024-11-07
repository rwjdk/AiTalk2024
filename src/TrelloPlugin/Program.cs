using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Shared;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloPlugin;
using TrelloPlugin.Models;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050

AzureOpenAiCredentials azureOpenAiCredentials = SecretManager.GetAzureOpenAiCredentials();
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o-mini", azureOpenAiCredentials.Endpoint, azureOpenAiCredentials.ApiKey);
builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();

TrelloCredentials trelloCredentials = SecretManager.GetTrelloCredentials();

TrelloClient trelloClient = new TrelloClient(trelloCredentials.ApiKey, trelloCredentials.Token);

Member currentUser = await trelloClient.GetTokenMemberAsync();

kernel.ImportPluginFromType<TimePlugin>();

kernel.ImportPluginFromObject(new TrelloInteractionPlugin(
    trelloClient: trelloClient,
    currentUser,
    currentBoard: new TrelloBoard
    {
        Id = "6634c3c9409306c0c08d99a2",
        Name = "RWJ Sandbox",
        Url = "https://trello.com/b/NmmS4RDU"
    }));

ChatCompletionAgent agent = new()
{
    Name = "TrelloAgent",
    Instructions = """
                   You are a Friendly Trello Agent that can exchange pleasantries and answer questions about trello in general and about the users Trello-boards. Other types of questions are not allowed!
                   You are given a board to work on so do not ask for that

                   Display rules:
                   - Please be short and precise in your answers
                   - If a trello object have an URL please add it as a MD link
                   - Only display the start and/or due date if there is any
                   - When a Label is displayed it should only display its name. Only if the name is empty should the color be mentioned in format '<color> Label' with quotation marks, '_' replaced with a space and capital first letter of each word
                   """,
    Kernel = kernel,
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            User = $"{currentUser.FullName}",
            Temperature = 0.5,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
};

var history = new ChatHistory();

Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    Console.Write("Question: ");
    var question = Console.ReadLine() ?? "";
    history.AddUserMessage(question);

    await foreach (var response in agent.InvokeStreamingAsync(history))
    {
        foreach (var content in response.Content ?? "")
        {
            Console.Write(content);
        }
    }

    history.Clear();
    Console.WriteLine();
    Console.WriteLine("*********************");
    Console.WriteLine();
}