using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace RetrievalAugmentedGeneration.Extensions;

public static class ChatHistoryExtensions
{
    public static void RemoveToolCalls(this ChatHistory chatHistory)
    {
        for (int i = chatHistory.Count - 1; i >= 0; i--)
        {
            ChatMessageContent content = chatHistory[i];

            if (content.Role == AuthorRole.Tool || content is OpenAIChatMessageContent openAiContent && openAiContent.ToolCalls != null)
            {
                chatHistory.RemoveAt(i);
            }
        }
    }
}