using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0110


var builder = Kernel.CreateBuilder();


Kernel kernel = builder.Build();
var agent = new ChatCompletionAgent
{
    Name = "MyOfflineAgent",
    Kernel = kernel,
    Instructions = "You are a friendly AI",
};

var history = new ChatHistory();

while (true)
{
    Console.Write("Question: ");
    var chatInput = Console.ReadLine() ?? "";
    history.AddUserMessage(chatInput);

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