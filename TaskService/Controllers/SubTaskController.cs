using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Controllers;

[ApiController]
[Route("api/task/{taskId:guid}/subtasks")]
public sealed class SubTaskController : ControllerBase
{
    private readonly TaskDbContext _db;

    public SubTaskController(TaskDbContext db) => _db = db;

    /// <summary>Lấy tất cả subtasks của một task</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SubTask>>> GetAll(Guid taskId, CancellationToken ct)
    {
        var subtasks = await _db.SubTasks
            .Where(s => s.TaskItemId == taskId)
            .AsNoTracking()
            .ToListAsync(ct);
        return Ok(subtasks);
    }

    /// <summary>Lấy subtask theo Id</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubTask>> GetById(Guid taskId, Guid id, CancellationToken ct)
    {
        var subtask = await _db.SubTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SubTaskId == id && s.TaskItemId == taskId, ct);
        return subtask is null ? NotFound() : Ok(subtask);
    }

    /// <summary>Tạo subtask mới cho task</summary>
    [HttpPost]
    public async Task<ActionResult<SubTask>> Create(Guid taskId, [FromBody] SubTask subtask, CancellationToken ct)
    {
        var taskExists = await _db.TaskItems.AnyAsync(t => t.TaskId == taskId, ct);
        if (!taskExists) return NotFound("Task không tồn tại.");

        subtask.SubTaskId = Guid.NewGuid();
        subtask.TaskItemId = taskId;
        subtask.CreatedAt = DateTime.UtcNow;
        _db.SubTasks.Add(subtask);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { taskId, id = subtask.SubTaskId }, subtask);
    }

    /// <summary>Đánh dấu subtask hoàn thành / chưa hoàn thành</summary>
    [HttpPatch("{id:guid}/toggle")]
    public async Task<ActionResult<SubTask>> Toggle(Guid taskId, Guid id, CancellationToken ct)
    {
        var subtask = await _db.SubTasks
            .FirstOrDefaultAsync(s => s.SubTaskId == id && s.TaskItemId == taskId, ct);
        if (subtask is null) return NotFound();

        subtask.IsCompleted = !subtask.IsCompleted;
        await _db.SaveChangesAsync(ct);
        return Ok(subtask);
    }

    /// <summary>Cập nhật subtask</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid taskId, Guid id, [FromBody] SubTask request, CancellationToken ct)
    {
        var subtask = await _db.SubTasks
            .FirstOrDefaultAsync(s => s.SubTaskId == id && s.TaskItemId == taskId, ct);
        if (subtask is null) return NotFound();

        subtask.Title = request.Title;
        subtask.IsCompleted = request.IsCompleted;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Xóa subtask</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id, CancellationToken ct)
    {
        var subtask = await _db.SubTasks
            .FirstOrDefaultAsync(s => s.SubTaskId == id && s.TaskItemId == taskId, ct);
        if (subtask is null) return NotFound();

        _db.SubTasks.Remove(subtask);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
