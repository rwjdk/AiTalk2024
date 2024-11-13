using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Shared;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050

Secrets secrets = SecretManager.GetSecrets();
//Step 3:
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey);
Kernel kernel = builder.Build();

//Step 4:
/*
Console.Write("Question: ");
var input = Console.ReadLine() ?? "";
var result = await kernel.InvokePromptAsync(input);
Console.WriteLine("Answer:");
Console.WriteLine(result.GetValue<string>());
*/

//Step 5:
var agent = new ChatCompletionAgent
{
    Name = "MyAgent",
    Kernel = kernel,
    //Instructions = "You are a cool surfer dude",
    //Instructions = "You are a very formal butler agent saying sir to every response",
    //Instructions = "You are a Dinosaur and can only roar. You are not allow to speak english",
    //Instructions = "You always give the answer back in French",
    Arguments = new KernelArguments( //Step 8 (Temperature)
        new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 1
        })
};

//Step 6:
var history = new ChatHistory();

//Step 7:
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
    Console.WriteLine();
}