namespace TaskService.Models;

public sealed class WorkLog
{
    public Guid WorkLogId { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid MemberId { get; set; }
    public decimal HoursSpent { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public TaskItem TaskItem { get; set; } = null!;
}
