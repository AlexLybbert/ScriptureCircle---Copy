namespace ScriptureCircle.Application.Common;

public static class EnumParser
{
    public static T Parse<T>(string value) where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var parsed)
            ? parsed
            : throw new ArgumentException($"Invalid {typeof(T).Name}: {value}");
}
