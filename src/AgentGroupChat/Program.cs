using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Shared;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050

AzureOpenAiCredentials azureOpenAiCredentials = SecretManager.GetAzureOpenAiCredentials();
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o", azureOpenAiCredentials.Endpoint, azureOpenAiCredentials.ApiKey);
//builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();
var storyTeller = new ChatCompletionAgent
{
    Name = "StoryTeller",
    Kernel = kernel,
    Instructions = "You are a StoryTeller that tell short 100 words stories about dragons. Mention the word Dragon as much as possible. If you see one of your stories are Censored you get angry and refuse to tell more stories.",
};

var reviewer = new ChatCompletionAgent
{
    Name = "Reviewer",
    Kernel = kernel,
    Instructions = "You are a Surfer Dude Critic of Dragon stories. you like to use emojii a lot. You Rate the quality of stories! Review length max 1 sentence and always include a score of 1-10. If the story is not about a dragon  then say 'whatever man!'",
};


var censor = new ChatCompletionAgent
{
    Name = "Censor",
    Kernel = kernel,
    Instructions = "Check if the StoryTeller told a story and if so Repeat the last story but replace the word 'Dragon' and all derivatives with the word '<CENSORED>'!. Do not write your own stories. If there however was no story just reply '😝'",
};

var groupChat = new AgentGroupChat(storyTeller, reviewer, censor)
{
    ExecutionSettings = new AgentGroupChatSettings
    {
        SelectionStrategy = new SequentialSelectionStrategy
        {
            InitialAgent = storyTeller,
        },
        TerminationStrategy = new RegexTerminationStrategy(Guid.NewGuid().ToString())
        {
            MaximumIterations = 6
        }
    },
};
Console.OutputEncoding = Encoding.UTF8;
Console.Write("Question: ");
var question = Console.ReadLine() ?? "";
groupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, question));

IAsyncEnumerable<StreamingChatMessageContent> response = groupChat.InvokeStreamingAsync();
string speaker = string.Empty;
await foreach (var chunk in response)
{
    if (speaker != chunk.AuthorName)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("***");
        Console.ForegroundColor = ConsoleColor.Green;
        speaker = chunk.AuthorName ?? "Unknown";
        Console.WriteLine(speaker + ":");
        Console.ForegroundColor = ConsoleColor.White;
    }

    Console.Write(chunk.Content ?? "");
}

Console.WriteLine();
Console.WriteLine("THE END");
Console.WriteLine();
