namespace CogRunner.Api.Startup;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CogRunner.Api.Configuration;

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

        services.AddSemanticKernel();
        services.AddChatAgent();

    }
}
