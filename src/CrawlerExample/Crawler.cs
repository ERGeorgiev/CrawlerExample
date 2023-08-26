using CrawlerExample.Configuration;
using CrawlerExample.Framework.Links;
using CrawlerExample.Framework.Page;

namespace CrawlerExample;

public class Crawler
{
    private readonly int _msDelayWaitForQueueItems = 100;
    private readonly ConcurrentUniqueLinkQueue _linkQueue = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _semaphoreSlimMaxCount;

    public Crawler(CrawlerConfiguration configuration, Uri startingLink)
    {
        _semaphoreSlimMaxCount = configuration.ThreadsMax;
        _semaphore = new(_semaphoreSlimMaxCount, _semaphoreSlimMaxCount);
        StartingLink = startingLink;
    }

    public IEnumerable<Uri> Results => _linkQueue.GetUniqueEnqueued();

    public int Count => _linkQueue.TotalUniqueEnqueued;

    public Uri StartingLink { get; }

    public async Task Run()
    {
        _linkQueue.Enqueue(new Uri[] { StartingLink });
        while (_linkQueue.Count > 0 || _semaphore.CurrentCount < _semaphoreSlimMaxCount)
        {
            if (_linkQueue.TryDequeue(out Uri? uri))
            {
                await _semaphore.WaitAsync();
                ExtractEnqueueAndRelease(uri);
            }
            else
            {
                await Task.Delay(_msDelayWaitForQueueItems);
            }
        }
    }

    private async void ExtractEnqueueAndRelease(Uri uri)
    {
        try
        {
            var pageLinkExtractor = new PageLinkExtractor(uri);
            var results = await pageLinkExtractor.Extract();
            _linkQueue.Enqueue(results);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}