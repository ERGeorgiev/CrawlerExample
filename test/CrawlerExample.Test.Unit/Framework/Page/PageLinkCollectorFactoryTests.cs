using CrawlerExample.Framework.Page;
using Microsoft.Extensions.Logging;

namespace CrawlerExample.Test.Unit.Framework.Page;

public class PageLinkCollectorFactoryTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<ILoggerFactory> _loggerFactory = new();
    private readonly PageLinkCollectorFactory _sut;

    public PageLinkCollectorFactoryTests()
    {
        _httpClient = new HttpClient();
        _sut = new(_httpClient, _loggerFactory.Object);
    }

    [Fact]
    public void Create_CollectorHasCorrectProperties()
    {
        var baseUri = UriHelper.GetRandomUri();
        var collectionUri = UriHelper.GetRandomUri();

        var result = _sut.Create(baseUri, collectionUri);

        Assert.Equal(baseUri.AbsoluteUri, result.BaseUri.AbsoluteUri);
        Assert.Equal(collectionUri.AbsoluteUri, result.CollectionUri.AbsoluteUri);
    }
}