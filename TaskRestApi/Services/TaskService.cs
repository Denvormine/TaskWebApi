using TaskRestApi.Data;
using TaskRestApi.DbContexts;
using TaskRestApi.HostedServices;

namespace TaskRestApi.Services;

public class TaskService
{
    private readonly TaskDbContext _taskDbContext;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public TaskService(
        TaskDbContext taskDbContext,
        IBackgroundTaskQueue backgroundTaskQueue
        )
    {
        _taskDbContext = taskDbContext;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public async ValueTask<SomeTask> AddAsync(CancellationToken cancellationToken)
    {
        SomeTask someTask = (await _taskDbContext.Tasks.AddAsync(new SomeTask(), cancellationToken)).Entity;
        await _taskDbContext.SaveChangesAsync(cancellationToken);
        await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(someTask);
        return someTask;
    }

    public async Task<SomeTask?> FindAsync(Guid guid, CancellationToken cancellationToken)
    {
        SomeTask? someTask = await _taskDbContext.Tasks.FindAsync(new object?[] { guid }, cancellationToken);
        return someTask;
    }
    
}