using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace ZeroToFirstPrompt.Website.Components.Pages;

[UsedImplicitly]
public partial class AiChatbot
{
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001
    private string? _question;
    private readonly Conversation _conversation = new();

    private ChatCompletionAgent? _agent;
    private readonly ChatHistory _chatHistory = new();

    [Inject]
    public required Kernel InjectedKernel { get; set; }

    protected override void OnInitialized()
    {
        _agent = new ChatCompletionAgent
        {
            Name = "MyAgent",
            Kernel = InjectedKernel,
            Instructions = "You are a friendly AI",
            Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings
                {
                    Temperature = 1,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
        };
    }

    private async Task AskQuestion()
    {
        if (string.IsNullOrWhiteSpace(_question))
        {
            return; //No Question
        }

        var item = new ConversationItem
        {
            Question = _question,
            Answer = string.Empty
        };
        _question = null;
        _conversation.Items.Add(item);

        _chatHistory.AddUserMessage(item.Question);

        await foreach (var response in _agent!.InvokeStreamingAsync(_chatHistory))
        {
            foreach (var content in response.Content ?? "")
            {
                item.Answer += content;
                StateHasChanged();
            }
        }
    }

    private class Conversation
    {
        public List<ConversationItem> Items { get; set; } = [];
    }

    private class ConversationItem
    {
        public required string Question { get; init; }
        public required string Answer { get; set; }
    }
}