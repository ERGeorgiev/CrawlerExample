using Microsoft.Extensions.Logging;

namespace CrawlerExample.Framework.Page;

public class PageLinkCollectorFactory : IPageLinkCollectorFactory
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerFactory _loggerFactory;

    public PageLinkCollectorFactory(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _loggerFactory = loggerFactory;
    }

    public IPageLinkCollector Create(Uri baseUri, Uri collectionUri)
    {
        return new PageLinkCollector(_httpClient, baseUri, collectionUri, _loggerFactory.CreateLogger<PageLinkCollector>());
    }
}