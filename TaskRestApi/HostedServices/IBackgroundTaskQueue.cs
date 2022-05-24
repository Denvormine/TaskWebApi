using TaskRestApi.Data;

namespace TaskRestApi.HostedServices;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(SomeTask workItem);

    ValueTask<SomeTask> DequeueAsync(CancellationToken cancellationToken);
}