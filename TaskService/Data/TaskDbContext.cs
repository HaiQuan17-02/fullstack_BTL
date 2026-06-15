using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService.Data;

public sealed class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<SubTask> SubTasks => Set<SubTask>();
    public DbSet<WorkLog> WorkLogs => Set<WorkLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Board>(entity =>
        {
            entity.ToTable("Boards");
            entity.HasKey(e => e.BoardId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("TaskItems");
            entity.HasKey(e => e.TaskId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ColorLabel).HasMaxLength(30);
            entity.Property(e => e.Priority).HasDefaultValue(1);
            entity.Property(e => e.CurrentStatus)
                .HasConversion<int>()
                .HasDefaultValue(TaskService.Models.TaskStatus.Backlog);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Board)
                .WithMany(e => e.TaskItems)
                .HasForeignKey(e => e.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubTask>(entity =>
        {
            entity.ToTable("SubTasks");
            entity.HasKey(e => e.SubTaskId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.TaskItem)
                .WithMany(e => e.SubTasks)
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkLog>(entity =>
        {
            entity.ToTable("WorkLogs");
            entity.HasKey(e => e.WorkLogId);
            entity.Property(e => e.MemberId).IsRequired();
            entity.Property(e => e.HoursSpent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.LoggedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.TaskItem)
                .WithMany(e => e.WorkLogs)
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
