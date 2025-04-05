namespace CogRunner.Api.Startup;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CogRunner.Api.Configuration;
using Microsoft.SemanticKernel.Agents;

public static class Bootstrapper
{
    public static async Task<CogDefinition> LoadCogAsync(string path = "cog.agent.yaml")
    {
        var yaml = await File.ReadAllTextAsync(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<CogDefinition>(yaml);
    }

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, CogDefinition cog)
    {
        services.AddSingleton(cog);

        services.Configure<AppSettings>(configuration);
        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidator>();

        services.AddSingleton<Kernel>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: settings.AzureOpenAI.ChatDeploymentName,
                endpoint: settings.AzureOpenAI.Endpoint,
                apiKey: settings.AzureOpenAI.ApiKey
            );

            return builder.Build();
        });

        services.AddSingleton<ChatCompletionAgent>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return new ChatCompletionAgent
            {
                Name = cog.Name,
                Instructions = cog.Prompt,
                Kernel = kernel
            };
        });
    }
}
