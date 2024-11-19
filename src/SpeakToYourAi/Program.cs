using System.Text;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.AudioToText;
using NAudio.Wave;
using System;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0050

Secrets secrets = SecretManager.GetSecrets();
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey);
builder.AddAzureOpenAIAudioToText("whisper", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey);

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

while (true)
{
    Console.WriteLine("Press any key to start recording mode...");
    Console.ReadKey();
    Console.WriteLine("Listening for your question... Press any key to stop.");
    var waveFormat = new WaveFormat(44100, 1);
    MemoryStream stream = new MemoryStream();
    await using (var waveStream = new WaveFileWriter(stream, waveFormat))
    {
        using (var waveIn = new WaveInEvent())
        {
            waveIn.WaveFormat = waveFormat;

            waveIn.DataAvailable += (_, eventArgs) =>
            {
                // ReSharper disable once AccessToDisposedClosure
                waveStream.Write(eventArgs.Buffer, 0, eventArgs.BytesRecorded);
            };

            waveIn.StartRecording();
            Console.ReadKey();
        }
    }

    IAudioToTextService audioService = kernel.GetRequiredService<IAudioToTextService>();

    byte[] audioBytes = stream.ToArray();
    var audioContent = new AudioContent(audioBytes.AsMemory(), "audio/wav");
    TextContent questionAsText = await audioService.GetTextContentAsync(audioContent);
    var question = questionAsText.Text!;
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