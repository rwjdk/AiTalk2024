using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureCosmosDBNoSQL;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using RetrievalAugmentedGeneration.RelewiseDocs;
using Shared;
using System.Text;
using Microsoft.SemanticKernel.Agents.History;
using RetrievalAugmentedGeneration.Extensions;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0020
Secrets secrets = SecretManager.GetSecrets();
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o-mini", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey); //Smaller model

//NEW
builder.AddAzureOpenAITextEmbeddingGeneration("text-embedding-ada-002", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey); //New!

Kernel kernel = builder.Build();

//NEW
ISemanticTextMemory semanticTextMemory = new MemoryBuilder()
    .WithTextEmbeddingGeneration(kernel.GetRequiredService<ITextEmbeddingGenerationService>())
    .WithMemoryStore(new AzureCosmosDBNoSQLMemoryStore(secrets.CosmosDbConnectionString, "relewise", 1536, VectorDataType.Float32, VectorIndexType.DiskANN))
    .Build();

//NEW
const bool importData = true;
const string vectorStoreCollection = "relewise-docs";
if (importData)
{
    await new RelewiseDocsImporter(vectorStoreCollection, semanticTextMemory).Import();
}

//NEW
kernel.ImportPluginFromObject(new RelewiseDocsPlugin(semanticTextMemory, vectorStoreCollection));

var agent = new ChatCompletionAgent
{
    Name = "RelewiseDocsAgent",
    Instructions = """
                   You are a Friendly Relewise Agent that can exchange pleasantries and can answer questions about how to use the Relewise API via its documentation.
                   Please only used information from memory plugins but please ask all of them for data
                   Please include all 'More info links' used at the bottom of the answer
                   Please only answer questions about Relewise. If you are ask about anything else please say 'I can only answer questions about Relewise' and if you do not know answer 'I do not know 😔'
                   """,
    Kernel = kernel,

    //NEW
    HistoryReducer = new ChatHistoryTruncationReducer(1),
    Arguments = new KernelArguments
    (
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
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

    //NEW
    history.RemoveToolCalls();
    await agent.ReduceAsync(history);

    Console.WriteLine();
    Console.WriteLine("*********************");
    Console.WriteLine();
}