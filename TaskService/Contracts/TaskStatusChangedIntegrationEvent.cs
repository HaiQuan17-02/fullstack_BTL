namespace TaskService.Contracts;

public sealed record TaskStatusChangedIntegrationEvent(
    Guid TaskId,
    string Title,
    int OldStatus,
    int NewStatus,
    DateTime ChangedAtUtc);
