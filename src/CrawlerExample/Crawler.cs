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
    private readonly ConcurrentUniqueUriQueue _linkQueue = new();
    private readonly PageRobotsReader _pageRobotsReader;
    private readonly HttpClient _httpClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Crawler> _logger;

    private DateTime _lastCrawl = DateTime.MinValue;

    public Crawler(CrawlerConfiguration configuration, Uri startingLink, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        StartingUri = startingLink;

        _httpClient = new HttpClient();
        _httpClient.AddUserAgent();
        _semaphoreSlimMaxCount = configuration.ThreadsMax;
        _semaphore = new(_semaphoreSlimMaxCount, _semaphoreSlimMaxCount);
        _pageRobotsReader = new PageRobotsReader(_httpClient, startingLink, _loggerFactory.CreateLogger<PageLinkCollector>());
        _logger = _loggerFactory.CreateLogger<Crawler>();
    }

    public IEnumerable<Uri> Results => _linkQueue.GetUniqueEnqueued();

    public int Count => _linkQueue.TotalUniqueEnqueued;

    public Uri StartingUri { get; }

    public RobotsFileConfiguration RobotsConfig { get; private set; } = new();

    public async Task Run()
    {
        RobotsConfig = await _pageRobotsReader.Get();
        _linkQueue.Configure(RobotsConfig);

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
            var pageLinkExtractor = new PageLinkCollector(_httpClient, StartingUri, uri, _loggerFactory.CreateLogger<PageLinkCollector>());
            var results = await pageLinkExtractor.Extract();
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