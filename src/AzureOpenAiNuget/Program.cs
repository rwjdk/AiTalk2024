using System.ClientModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Shared;

var secrets = SecretManager.GetSecrets();

var endpoint = new Uri(secrets.AzureOpenAiEndpoint);
var apiKeyCredential = new ApiKeyCredential(secrets.AzureOpenAiApiKey);
Azure.AI.OpenAI.AzureOpenAIClient client = new AzureOpenAIClient(endpoint, apiKeyCredential);
var chatClient = client.GetChatClient("gpt-4o-mini");

//await Chat();
ChatWithFunctionCalling();

async Task Chat()
{
    List<ChatMessage> history = [];
    Console.OutputEncoding = Encoding.UTF8;
    while (true)
    {
        Console.Write("> ");
        var question = Console.ReadLine() ?? "";
        history.Add(ChatMessage.CreateUserMessage(question));

        await foreach (var update in chatClient.CompleteChatStreamingAsync(history))
        {
            foreach (var content in update.ContentUpdate)
            {
                Console.Write(content.Text);
            }
        }

        Console.WriteLine();
        Console.WriteLine("*********************");
        Console.WriteLine();
    }
}

void ChatWithFunctionCalling()
{
    static int HowOldAreMyFriend()
    {
        return 42;
    }

    var chatCompletionOptions = new ChatCompletionOptions();
    chatCompletionOptions.Tools.Add(ChatTool.CreateFunctionTool(nameof(HowOldAreMyFriend), "Can give age of my friends"));

    List<ChatMessage> history = [];
    Console.OutputEncoding = Encoding.UTF8;
    while (true)
    {
        Console.Write("> ");
        var question = Console.ReadLine() ?? "";
        history.Add(ChatMessage.CreateUserMessage(question));

        bool requiresAction;

        do
        {
            requiresAction = false;
            ChatCompletion completion = chatClient.CompleteChat(history, chatCompletionOptions);

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                    {
                        // Add the assistant message to the conversation history.
                        history.Add(new AssistantChatMessage(completion));
                        break;
                    }

                case ChatFinishReason.ToolCalls:
                    {
                        // First, add the assistant message with tool calls to the conversation history.
                        history.Add(new AssistantChatMessage(completion));

                        // Then, add a new tool message for each tool call that is resolved.
                        foreach (ChatToolCall toolCall in completion.ToolCalls)
                        {
                            switch (toolCall.FunctionName)
                            {
                                case nameof(HowOldAreMyFriend):
                                    {
                                        string toolResult = HowOldAreMyFriend().ToString();
                                        history.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                        break;
                                    }
                                default:
                                    {
                                        // Handle other unexpected calls.
                                        throw new NotImplementedException();
                                    }
                            }
                        }

                        requiresAction = true;
                        break;
                    }

                case ChatFinishReason.Length:
                    throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                case ChatFinishReason.ContentFilter:
                    throw new NotImplementedException("Omitted content due to a content filter flag.");

                case ChatFinishReason.FunctionCall:
                    throw new NotImplementedException("Deprecated in favor of tool calls.");

                default:
                    throw new NotImplementedException(completion.FinishReason.ToString());
            }
        } while (requiresAction);


        var chatMessageContent = history.Last().Content;
        foreach (var part in chatMessageContent)
        {
            Console.Write(part.Text);
        }

        Console.WriteLine();
        Console.WriteLine("*********************");
        Console.WriteLine();
    }
}