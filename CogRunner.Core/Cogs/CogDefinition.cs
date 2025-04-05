public record CogDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public ExposeConfig Expose { get; set; } = new ExposeConfig
    {
        Path = "/cog",
        Method = "POST"
    };
}
