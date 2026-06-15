using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Repositories;

public sealed class TaskItemRepository : ITaskItemRepository
{
    private readonly TaskDbContext _context;

    public TaskItemRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.TaskItems
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.TaskItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TaskId == id, cancellationToken);

    public async Task<TaskItem> AddAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        _context.TaskItems.Add(taskItem);
        await _context.SaveChangesAsync(cancellationToken);
        return taskItem;
    }

    public async Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        _context.TaskItems.Update(taskItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.TaskId == id, cancellationToken);
        if (task is null)
        {
            return;
        }

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
