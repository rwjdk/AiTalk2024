using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace RetrievalAugmentedGeneration.RelewiseDocs;

#pragma warning disable SKEXP0001

public class RelewiseDocsPlugin(ISemanticTextMemory textMemory, string collection)
{
    [KernelFunction("get_informantion_about_relewise")]
    public async Task<string[]> Get(string question)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Plugin Called with question: " + question);
        Console.ForegroundColor = ConsoleColor.White;

        var memories = textMemory.SearchAsync(collection, question, limit: 3, minRelevanceScore: 0.75);
        var list = new List<string>();
        await foreach (var memory in memories)
        {
            list.Add(memory.Metadata.Text + $" [More Info link: {memory.Metadata.AdditionalMetadata}]");
        }

        return list.ToArray();
    }
}