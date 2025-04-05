namespace CogRunner.Api.Configuration;

public class AppSettings
{
    public AzureOpenAIConfig AzureOpenAI { get; set; } = new();
}

public class AzureOpenAIConfig
{
    public string Endpoint { get; set; } = "";
    public string ChatDeploymentName { get; set; } = "";
    public string ApiKey { get; set; } = "";
}