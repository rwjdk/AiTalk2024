using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Shared;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0070

Secrets secrets = SecretManager.GetSecrets();

string azureOpenAiEndpoint = secrets.AzureOpenAiEndpoint;
string azureOpenAiApiKey = secrets.AzureOpenAiApiKey; //Todo - Should not be directly in real code!!! 
const string modelDeploymentName = "gpt-4o";

//Step 3:
var builder = Kernel.CreateBuilder();
//Step 3 - Azure OpenAI
builder.AddAzureOpenAIChatCompletion(modelDeploymentName, azureOpenAiEndpoint, azureOpenAiApiKey);

//Step 8a - Google Gemini
//builder.AddGoogleAIGeminiChatCompletion("gemini-1.5-flash", secrets.GoogleGeminiApiKey);
//Step 8b - Offline
//builder.AddOllamaChatCompletion("llama3.1", new Uri("http://localhost:11434")); //Get Models: https://ollama.com/library

Kernel kernel = builder.Build();

//Step 4:
/*
Console.Write("> ");
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
    Instructions = "You are a nice AI Agent",
    //Instructions = "You are a cool surfer dude",
    //Instructions = "You are a very formal butler agent saying sir to every response",
    //Instructions = "You are a Dinosaur and can only roar. You are not allow to speak english",
    //Instructions = "You always give the answer back in French",
};

//Step 6:
var history = new ChatHistory();

//Step 7:
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