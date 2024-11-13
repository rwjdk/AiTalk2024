using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Shared;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050

Secrets secrets = SecretManager.GetSecrets();
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey);
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

Console.WriteLine("Meet our agents");
Console.WriteLine("- John is our StoryTeller; he love telling stories about dragons... But it a bit edgy if people mess with his stories");
Console.WriteLine("- Wayne is our Reviewer... When he does not surf 🏄 he rate dragon stories");
Console.WriteLine("- Mr. Smith is a Censor... His biggest goal in life if to censor stories... Especially about dragons!");
Console.WriteLine("Press any key to see how these three get along if you drop them into a group-chat...");
Console.ReadKey();
Console.Clear();

Console.OutputEncoding = Encoding.UTF8;
Console.Write("What should the story be about (other than dragons of cause...): ");
var question = Console.ReadLine() ?? "";
groupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, "tell a story about: "+question));

IAsyncEnumerable<StreamingChatMessageContent> response = groupChat.InvokeStreamingAsync();
string speaker = string.Empty;
await foreach (var chunk in response)
{
    if (speaker != chunk.AuthorName)
    {
        if (!string.IsNullOrWhiteSpace(speaker))
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

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