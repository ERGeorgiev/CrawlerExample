using CrawlerExample.Configuration;
using CrawlerExample.Framework.Links;
using CrawlerExample.Framework.Page;

namespace CrawlerExample;

public class Crawler
{
    private readonly object _runLock = new();
    private readonly int _msDelayWaitForQueueItems = 100;
    private readonly ConcurrentUniqueLinkQueue _linkQueue = new();
    private readonly ExtractAndQueueService _pageLinkExtractorService;
    private readonly Task? _extractorDoneNotificationTask;

    public Crawler(CrawlerConfiguration configuration)
    {
        _pageLinkExtractorService = new ExtractAndQueueService(_linkQueue, configuration.ThreadsMax);
    }

    public IEnumerable<Uri> Results => _linkQueue.GetUniqueEnqueued();

    public int Count => _linkQueue.TotalUniqueEnqueued;

    public async Task Run(Uri startingLink)
    {
        bool lockTaken = false;
        try
        {
            Monitor.TryEnter(_runLock, 0, ref lockTaken);
            if (lockTaken)
            {
                _linkQueue.Enqueue(new Uri[] { startingLink });
                while (_linkQueue.Count > 0 || _pageLinkExtractorService.Running)
                {
                    if (_linkQueue.Count > 0)
                    {
                        _pageLinkExtractorService.BeginExtraction();
                    }
                    else
                    {
                        await Task.Delay(_msDelayWaitForQueueItems);
                    }
                }
            }
            else
            {

            }
            Console.WriteLine("Completed");
        }
        finally
        {
            if (lockTaken) Monitor.Exit(_runLock);
        }
    }
}