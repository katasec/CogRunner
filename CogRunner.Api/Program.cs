using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// Load cog.agent.yaml
var yaml = await File.ReadAllTextAsync("cog.agent.yaml");
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();
var cog = deserializer.Deserialize<CogDefinition>(yaml);


// Retrieve environment variables
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT environment variable is not set.");
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")
    ?? throw new InvalidOperationException("AZURE_OPENAI_KEY environment variable is not set.");

// Boot Semantic Kernel
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o",
        endpoint: endpoint,
        apiKey: apiKey
    ).Build();

var agent = new ChatCompletionAgent
{
    Name = cog.Name,
    Instructions = cog.Prompt,
    Kernel = kernel
};


// Expose HTTP endpoint
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost(cog.Expose.Path, async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var input = await reader.ReadToEndAsync();

    var chat = new AgentGroupChat(agent);

    chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
    var result = chat.InvokeAsync();

    await foreach (var response in result)
    {
        return Results.Ok(response.Content);
    }

    return Results.Problem("No response generated.");
});

app.Run();
