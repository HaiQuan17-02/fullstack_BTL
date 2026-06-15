namespace TaskService.Models;

public sealed class TaskItem
{
    public Guid TaskId { get; set; }
    public Guid BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public string? ColorLabel { get; set; }
    public DateTime? Deadline { get; set; }
    public TaskStatus CurrentStatus { get; set; } = TaskStatus.Backlog;
    public Guid? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Board Board { get; set; } = null!;
    public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
    public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
}
