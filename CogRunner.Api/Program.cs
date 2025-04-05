using CogRunner.Api.Startup;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;


// Bootstrap App
var builder = WebApplication.CreateBuilder(args);
var cog = await Bootstrapper.LoadCogAsync();
Bootstrapper.RegisterServices(builder.Services, builder.Configuration, cog);
var app = builder.Build();

// Get Agent
var agent = app.Services.GetRequiredService<ChatCompletionAgent>();

// Expose endpoint
app.MapPost(cog.Expose.Path, async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var input = await reader.ReadToEndAsync();

    var chat = new AgentGroupChat(agent);
    chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

    await foreach (var response in chat.InvokeAsync())
    {
        return Results.Ok(response.Content);
    }

    return Results.Problem("No response generated.");
});

app.Run();
