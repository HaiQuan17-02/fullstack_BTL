namespace TaskService.Models;

public sealed class Board
{
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
}
