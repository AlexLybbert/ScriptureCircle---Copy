namespace ScriptureCircle.Shared.Subscriptions;

public sealed record SubscriptionDto(
    Guid Id,
    string Name,
    string Plan,
    bool IsActive,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndsAt);

public sealed record UpsertSubscriptionRequest(string Name, string Plan, bool IsActive, DateTimeOffset? EndsAt);
