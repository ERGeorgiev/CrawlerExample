using CrawlerExample.Configuration;
using CrawlerExample.Framework.Links;
using CrawlerExample.Framework.Page;
using Microsoft.Extensions.Logging;

namespace CrawlerExample;

public class Crawler
{
    private readonly SemaphoreSlim _semaphore;
    private readonly int _semaphoreSlimMaxCount;
    private readonly int _delayWaitForQueueItemsMs = 100;
    private readonly IConcurrentUniqueUriQueue _linkQueue;
    private readonly IPageLinkCollectorFactory _collectorFactory;
    private readonly HttpClient _httpClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Crawler> _logger;

    private DateTime _lastCrawl = DateTime.MinValue;

    public Crawler(
        CrawlerConfiguration configuration,
        HttpClient httpClient,
        IConcurrentUniqueUriQueue queue,
        IPageLinkCollectorFactory collectorFactory,
        Uri startingLink,
        ILoggerFactory loggerFactory)
    {
        _linkQueue = queue;
        _collectorFactory = collectorFactory;
        _loggerFactory = loggerFactory;
        StartingUri = startingLink;

        _httpClient = httpClient;
        _httpClient.AddUserAgent();
        _semaphoreSlimMaxCount = configuration.ThreadsMax;
        _semaphore = new(_semaphoreSlimMaxCount, _semaphoreSlimMaxCount);
        _logger = _loggerFactory.CreateLogger<Crawler>();
    }

    public Uri StartingUri { get; }

    public RobotsFileConfiguration RobotsConfig { get; private set; } = new();

    public async Task Run()
    {
        _linkQueue.Enqueue(new Uri[] { StartingUri });

        while (_linkQueue.Count > 0 || _semaphore.CurrentCount < _semaphoreSlimMaxCount)
        {
            await EnsureCrawlDelay();
            if (_linkQueue.TryDequeue(out Uri? uri))
            {
                await _semaphore.WaitAsync();
                _lastCrawl = DateTime.Now;
                CollectEnqueueAndRelease(uri);
            }
            else
            {
                await Task.Delay(_delayWaitForQueueItemsMs);
            }
        }
    }

    private async Task EnsureCrawlDelay()
    {
        var nextCall = _lastCrawl + TimeSpan.FromMilliseconds(RobotsConfig.CrawlDelay);
        var timeToWait = nextCall - DateTime.Now;
        if (timeToWait.TotalMilliseconds > 0)
        {
            await Task.Delay(timeToWait);
        }
    }

    private async void CollectEnqueueAndRelease(Uri uri)
    {
        try
        {
            var collector = _collectorFactory.Create(StartingUri, uri);
            var results = await collector.Collect();
            _linkQueue.Enqueue(results);
        }
        catch
        {
            _logger.LogError("Failed to collect from uri '{uri}'", uri.AbsoluteUri);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}