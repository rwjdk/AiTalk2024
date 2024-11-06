using System.Text;
using HelloPlugin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Shared;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050

AzureOpenAiCredentials azureOpenAiCredentials = SecretManager.GetAzureOpenAiCredentials();
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o-mini", azureOpenAiCredentials.Endpoint, azureOpenAiCredentials.ApiKey);
//builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();

kernel.ImportPluginFromObject(new MyFirstPlugin());

var agent = new ChatCompletionAgent
{
    Name = "MyAgent",
    Kernel = kernel,
    Instructions = """
                   You are a friendly AI Agent that know about Aarhus .NET UserGroup, also known. as ANUG. 
                   Do not use you general knowledge, but instead 'get_anug_history' when answering questions
                   Keep the responses short and concise",
                   """,
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.5,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        })
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

    Console.WriteLine();
    Console.WriteLine("*********************");
}