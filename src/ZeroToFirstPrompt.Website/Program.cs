using Microsoft.SemanticKernel;
using Shared;
using ZeroToFirstPrompt.Website.Components.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

Secrets secrets = SecretManager.GetSecrets();
//AI: BEGIN
builder.Services.AddKernel();
builder.Services.AddAzureOpenAIChatCompletion("gpt-4o", secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiApiKey);
//AI: END

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();