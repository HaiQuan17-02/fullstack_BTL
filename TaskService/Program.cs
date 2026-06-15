using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;
using TaskService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskDbConnection")));

builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    await dbContext.Database.MigrateAsync();

    if (!await dbContext.Boards.AnyAsync())
    {
        var board = new Board
        {
            Name = "Demo Board",
            Description = "Bảng dữ liệu mẫu để test API và giao diện.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Boards.Add(board);
        await dbContext.SaveChangesAsync();

        var tasks = new[]
        {
            new TaskItem
            {
                BoardId = board.BoardId,
                Title = "Thiết kế API task",
                Description = "Tạo endpoint CRUD cho nhiệm vụ.",
                Priority = 2,
                ColorLabel = "#3B82F6",
                CurrentStatus = TaskService.Models.TaskStatus.ToDo,
                AssigneeId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(3),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                BoardId = board.BoardId,
                Title = "Test giao diện",
                Description = "Kiểm tra hiển thị danh sách task.",
                Priority = 1,
                ColorLabel = "#10B981",
                CurrentStatus = TaskService.Models.TaskStatus.InProgress,
                AssigneeId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                BoardId = board.BoardId,
                Title = "Hoàn thiện release",
                Description = "Chuẩn bị dữ liệu mẫu cho demo.",
                Priority = 3,
                ColorLabel = "#F59E0B",
                CurrentStatus = TaskService.Models.TaskStatus.Backlog,
                AssigneeId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        dbContext.TaskItems.AddRange(tasks);
        await dbContext.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
