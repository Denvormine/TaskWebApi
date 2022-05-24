using System.Threading.Channels;
using TaskRestApi.Data;

namespace TaskRestApi.HostedServices;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<SomeTask> _queue;

    public BackgroundTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<SomeTask>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(SomeTask workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<SomeTask> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}