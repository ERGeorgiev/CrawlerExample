using Microsoft.Extensions.Logging;

namespace CrawlerExample.Framework.Page;

public class PageLinkCollector
{
    private readonly ILogger<PageLinkCollector> _logger;

    public PageLinkCollector(Uri baseUri, Uri collectionUri, ILogger<PageLinkCollector> logger)
    {
        _logger = logger;
        BaseUri = baseUri;
        CollectionUri = collectionUri;
    }

    public Uri BaseUri { get; set; }

    public Uri CollectionUri { get; set; }

    public async Task<IEnumerable<Uri>> Extract()
    {
        // ToDo: Print visited
        // ToDo: Print collected
        _logger.LogInformation("test");
        await Task.Delay(1000);
        return new List<Uri>() { new Uri($"https://www.youtube.com/{Random.Shared.Next(0, 100)}"), new Uri($"https://www.youtube.com/{Random.Shared.Next(0, 100)}") };
    }
}