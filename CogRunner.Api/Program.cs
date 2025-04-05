using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;



var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Load cog.yaml manually
var yaml = await File.ReadAllTextAsync("cog.agent.yaml");
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();
var cog = deserializer.Deserialize<CogDefinition>(yaml);



var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
if (string.IsNullOrEmpty(endpoint))
{
    throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT environment variable is not set.");
}

var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("AZURE_OPENAI_KEY environment variable is not set.");
}





// Boot Semantic Kernel
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o",
        endpoint: endpoint,
        apiKey: apiKey
    ).Build();


// Handlebars prompt config
var templateConfig = new PromptTemplateConfig
{
    Name = cog.Name,
    Template = cog.Prompt,
    TemplateFormat = "handlebars",
    InputVariables =
    [
        new() {
            Name = "input",
            Description = "The input question for the assistant."
        }
    ]
};


var factory = new HandlebarsPromptTemplateFactory();
var template = factory.Create(templateConfig);
var promptFunction = kernel.CreateFunctionFromPrompt(templateConfig, factory);


// Expose HTTP endpoint
app.MapPost(cog.Expose.Path, async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var input = await reader.ReadToEndAsync();


    var rendered = await template.RenderAsync(kernel, new KernelArguments
    {
        ["input"] = input
    });

    Console.WriteLine($"Rendered Prompt:\n{rendered}");


    var args = new KernelArguments
    {
        ["input"] = input
    };

    var result = await kernel.InvokeAsync(promptFunction, args);
    return Results.Ok(result.GetValue<string>());
});


app.Run();
