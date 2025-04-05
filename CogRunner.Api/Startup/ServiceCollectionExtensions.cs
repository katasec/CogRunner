using CogRunner.Api.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace CogRunner.Api.Startup;



public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration);
        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidator>();
        return services;
    }

    public static IServiceCollection AddSemanticKernel(this IServiceCollection services)
    {
        services.AddSingleton<Kernel>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: settings.AzureOpenAI.ChatDeploymentName,
                endpoint: settings.AzureOpenAI.Endpoint,
                apiKey: settings.AzureOpenAI.ApiKey
            );

            var kernel = builder.Build();

            return kernel;
        });

        services.AddSingleton(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var cog = sp.GetRequiredService<CogDefinition>();

            return new ChatCompletionAgent
            {
                Name = cog.Name,
                Instructions = cog.Prompt,
                Kernel = kernel
            };
        });

        return services;
    }
}