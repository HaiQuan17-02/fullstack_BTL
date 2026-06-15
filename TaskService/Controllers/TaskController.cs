using MassTransit;
using Microsoft.AspNetCore.Mvc;
using TaskService.Contracts;
using TaskService.Models;
using TaskService.Repositories;

namespace TaskService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TaskController : ControllerBase
{
    private readonly ITaskItemRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public TaskController(ITaskItemRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TaskItem>>> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await _repository.GetAllAsync(cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskItem>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItem taskItem, CancellationToken cancellationToken)
    {
        var created = await _repository.AddAsync(taskItem, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TaskItem request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.Priority = request.Priority;
        existing.ColorLabel = request.ColorLabel;
        existing.Deadline = request.Deadline;
        existing.CurrentStatus = request.CurrentStatus;
        existing.AssigneeId = request.AssigneeId;
        existing.BoardId = request.BoardId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var oldStatus = existing.CurrentStatus;
        existing.CurrentStatus = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, cancellationToken);

        await _publishEndpoint.Publish(new TaskStatusChangedIntegrationEvent(
            existing.TaskId,
            existing.Title,
            (int)oldStatus,
            (int)existing.CurrentStatus,
            DateTime.UtcNow));

        return Ok(existing);
    }

    public sealed record UpdateTaskStatusRequest(TaskService.Models.TaskStatus Status);
}
