using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Controllers;

[ApiController]
[Route("api/task/{taskId:guid}/worklogs")]
public sealed class WorkLogController : ControllerBase
{
    private readonly TaskDbContext _db;

    public WorkLogController(TaskDbContext db) => _db = db;

    /// <summary>Lấy tất cả work logs của một task</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<WorkLog>>> GetAll(Guid taskId, CancellationToken ct)
    {
        var logs = await _db.WorkLogs
            .Where(w => w.TaskItemId == taskId)
            .AsNoTracking()
            .OrderByDescending(w => w.LoggedAt)
            .ToListAsync(ct);
        return Ok(logs);
    }

    /// <summary>Lấy work log theo Id</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkLog>> GetById(Guid taskId, Guid id, CancellationToken ct)
    {
        var log = await _db.WorkLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkLogId == id && w.TaskItemId == taskId, ct);
        return log is null ? NotFound() : Ok(log);
    }

    /// <summary>Ghi nhận thời gian làm việc cho task</summary>
    [HttpPost]
    public async Task<ActionResult<WorkLog>> Create(Guid taskId, [FromBody] WorkLog workLog, CancellationToken ct)
    {
        var taskExists = await _db.TaskItems.AnyAsync(t => t.TaskId == taskId, ct);
        if (!taskExists) return NotFound("Task không tồn tại.");

        workLog.WorkLogId = Guid.NewGuid();
        workLog.TaskItemId = taskId;
        workLog.LoggedAt = DateTime.UtcNow;
        _db.WorkLogs.Add(workLog);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { taskId, id = workLog.WorkLogId }, workLog);
    }

    /// <summary>Cập nhật work log</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid taskId, Guid id, [FromBody] WorkLog request, CancellationToken ct)
    {
        var log = await _db.WorkLogs
            .FirstOrDefaultAsync(w => w.WorkLogId == id && w.TaskItemId == taskId, ct);
        if (log is null) return NotFound();

        log.HoursSpent = request.HoursSpent;
        log.Note = request.Note;
        log.MemberId = request.MemberId;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Xóa work log</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id, CancellationToken ct)
    {
        var log = await _db.WorkLogs
            .FirstOrDefaultAsync(w => w.WorkLogId == id && w.TaskItemId == taskId, ct);
        if (log is null) return NotFound();

        _db.WorkLogs.Remove(log);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Tổng số giờ đã log cho task</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary(Guid taskId, CancellationToken ct)
    {
        var total = await _db.WorkLogs
            .Where(w => w.TaskItemId == taskId)
            .SumAsync(w => w.HoursSpent, ct);
        return Ok(new { taskId, totalHoursSpent = total });
    }
}
