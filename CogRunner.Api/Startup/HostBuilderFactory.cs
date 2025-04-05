namespace CogRunner.Api.Startup;



public static class HostBuilderFactory
{
    public static IHost Build()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureServices((context, services) =>
            {
                services.AddAppSettings(context.Configuration);
                services.AddSemanticKernel();
            })
            .Build();
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configBuilder)
    {
        configBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>();
    }
}
