using TaskRestApi.Data;
using TaskRestApi.DbContexts;
using TaskStatus = TaskRestApi.Data.TaskStatus;

namespace TaskRestApi.HostedServices;

public class TaskUpdaterHostedService : BackgroundService
{
    private readonly ILogger<TaskUpdaterHostedService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly int MillisecondsToFinishTask;
    private readonly IBackgroundTaskQueue _taskQueue;
    public TaskUpdaterHostedService(
        IBackgroundTaskQueue taskQueue, 
        ILogger<TaskUpdaterHostedService> logger,
        IServiceScopeFactory serviceScopeFactory,
        int millisecondsToFinishTask)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        MillisecondsToFinishTask = millisecondsToFinishTask;
        Initialize();
    }

    private void Initialize()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetService<TaskDbContext>()!;
        foreach (var someTask in dbContext.Tasks.Where(x => x.Status != TaskStatus.Created).ToList())
        {
            _taskQueue.QueueBackgroundWorkItemAsync(someTask);
        }
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await BackgroundProcessing(cancellationToken);
    }

    private async Task BackgroundProcessing(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var someTask = await _taskQueue.DequeueAsync(cancellationToken);
            ProcessTask(someTask);
        }
    }

    public async Task ProcessTask(SomeTask someTask)
    {
        switch (someTask.Status)
        {
            case TaskStatus.Created:
            {
                using var scope = _serviceScopeFactory.CreateScope();
                await using var dbContext = scope.ServiceProvider.GetService<TaskDbContext>()!;
                dbContext.Tasks.Attach(someTask);
                someTask.Start();
                dbContext.Update(someTask);
                await dbContext.SaveChangesAsync();
                await _taskQueue.QueueBackgroundWorkItemAsync(someTask);
                break;
            }
            case TaskStatus.Running:
            {
                using var scope = _serviceScopeFactory.CreateScope();
                await using var dbContext = scope.ServiceProvider.GetService<TaskDbContext>()!;
                dbContext.Tasks.Attach(someTask);
                someTask.Finish();
                int timeToWait = MillisecondsToFinishTask - (DateTime.UtcNow - someTask.DateTime).Milliseconds;
                await Task.Delay(Math.Max(0, timeToWait));
                dbContext.Update(someTask);
                await dbContext.SaveChangesAsync();
                break;
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}