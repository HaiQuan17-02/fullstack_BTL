using TaskService.Models;

namespace TaskService.Repositories;

public interface ITaskItemRepository
{
    Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskItem> AddAsync(TaskItem taskItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
