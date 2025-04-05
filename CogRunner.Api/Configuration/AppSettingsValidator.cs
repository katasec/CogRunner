using Microsoft.Extensions.Options;

namespace CogRunner.Api.Configuration;


public class AppSettingsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings settings)
    {
        var missing = new List<string>();

        if (string.IsNullOrEmpty(settings.AzureOpenAI.Endpoint))
            missing.Add("AzureOpenAI.Endpoint");

        if (string.IsNullOrEmpty(settings.AzureOpenAI.ChatDeploymentName))
            missing.Add("AzureOpenAI.ChatDeploymentName");

        if (string.IsNullOrEmpty(settings.AzureOpenAI.ApiKey))
            missing.Add("AzureOpenAI.ApiKey");

        if (missing.Count != 0)
        {
            var message = "Missing or empty configuration values: " + string.Join(",", missing);
            return ValidateOptionsResult.Fail(message);
        }

        return ValidateOptionsResult.Success;
    }
}