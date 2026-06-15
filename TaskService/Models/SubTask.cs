namespace TaskService.Models;

public sealed class SubTask
{
    public Guid SubTaskId { get; set; }
    public Guid TaskItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TaskItem TaskItem { get; set; } = null!;
}
