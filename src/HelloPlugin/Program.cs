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

//Files Sample Setup
const string sampleRoot = "C:\\HelloPlugin";
if (!Directory.Exists(sampleRoot))
{
    Directory.CreateDirectory(sampleRoot);
}

Secrets secrets = SecretManager.GetSecrets();
var builder = Kernel.CreateBuilder();
builder.Services.AddSingleton<IAutoFunctionInvocationFilter, AutoInvocationFilter>();
builder.AddAzureOpenAIChatCompletion("gpt-4o-mini", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey);
//builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();

kernel.ImportPluginFromType<TimePlugin>();
kernel.ImportPluginFromObject(new MyFirstPlugin(sampleRoot));

var agent = new ChatCompletionAgent
{
    Name = "MyFilesAgent",
    Kernel = kernel,
    Instructions = $"You are File Manager that can create and list files and folders. " +
                   $"When you create files and folder you need to give the full path based" +
                   $" on this root folder: {sampleRoot}",
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
    Console.Write("> ");
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
    Console.WriteLine();
}