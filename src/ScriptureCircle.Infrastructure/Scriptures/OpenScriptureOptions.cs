namespace ScriptureCircle.Infrastructure.Scriptures;

public sealed class OpenScriptureOptions
{
    public const string SectionName = "OpenScripture";
    public string BaseUrl { get; init; } = "https://openscriptureapi.org/api/scriptures/v1/lds/en/";
}
