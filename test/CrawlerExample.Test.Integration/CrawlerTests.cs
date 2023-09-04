using CrawlerExample.Configuration;
using CrawlerExample.Framework.Links;
using CrawlerExample.Framework.Page;
using Microsoft.Extensions.Logging;

namespace CrawlerExample.Test.Unit;

// ToDo: Finish Integration Tests
public class CrawlerTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<ILoggerFactory> _loggerMock = new();
    private readonly Mock<IConcurrentUniqueUriQueue> _queueMock = new();
    private readonly Mock<IPageLinkCollectorFactory> _collectorFactory = new();
    private readonly Mock<IPageLinkCollector> _collector = new();
    private readonly List<Uri> _collectorReturnedUris = new();
    private readonly Crawler _sut;
    private Uri _startingUri;

    public CrawlerTests()
    {
        _startingUri = new Uri("https://www.placeholder.com");
        // _httpClient = new HttpClient(_mockHttpMessageHandler);
        _sut = new(new CrawlerConfiguration(), _httpClient, _queueMock.Object, _collectorFactory.Object, _startingUri, _loggerMock.Object);

        _collector.Setup(c => c.Collect()).ReturnsAsync(_collectorReturnedUris);
        _collectorFactory.Setup(c => c.Create(It.IsAny<Uri>(), It.IsAny<Uri>())).Returns(_collector.Object);
    }

    [Fact]
    public void Run_ExampleHtml_ExpectedOutput()
    {
    }
}