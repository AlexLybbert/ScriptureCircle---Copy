namespace ScriptureCircle.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
    string DisplayName { get; }
}
