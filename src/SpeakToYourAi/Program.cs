using System.Text;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Shared;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050

Secrets secrets = SecretManager.GetSecrets();
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey);
Kernel kernel = builder.Build();

var agent = new ChatCompletionAgent
{
    Name = "MyAgent",
    Kernel = kernel,
    Instructions = "You are nice AI",
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 1,
        })
};

var history = new ChatHistory();
Console.OutputEncoding = Encoding.UTF8;

var speechConfig = SpeechConfig.FromSubscription(secrets.AzureSpeechApiKey, "swedencentral");
while (true)
{
    Console.WriteLine("Press any key to ask you question...");
    Console.ReadKey();
    Console.WriteLine("Listening for your question...");

    speechConfig.SpeechRecognitionLanguage = "da-DK";
    var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
    var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
    SpeechRecognitionResult result = await speechRecognizer.RecognizeOnceAsync();
    var question = result.Text;
    Console.WriteLine("Question: " + question);
    if (string.IsNullOrWhiteSpace(question))
    {
        continue;
    }

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