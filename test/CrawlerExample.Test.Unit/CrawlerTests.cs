using CrawlerExample.Configuration;
using CrawlerExample.Framework.Links;
using CrawlerExample.Framework.Page;
using Microsoft.Extensions.Logging;

namespace CrawlerExample.Test.Unit;

public class CrawlerTests
{
    private readonly HttpClient _httpClient;
    private readonly MockHttpMessageHandler _mockHttpMessageHandler = new();
    private readonly Mock<ILoggerFactory> _loggerMock = new();
    private readonly Mock<IConcurrentUniqueUriQueue> _queueMock = new();
    private readonly Mock<IPageLinkCollectorFactory> _collectorFactory = new();
    private readonly Mock<IPageLinkCollector> _collector = new();
    private readonly List<Uri> _collectorReturnedUris = new();
    private readonly Crawler _sut;
    private Uri _startingUri;

    public CrawlerTests()
    {
        _startingUri = UriHelper.GetRandomUri();
        _httpClient = new HttpClient(_mockHttpMessageHandler);
        _sut = new(new CrawlerConfiguration(), _httpClient, _queueMock.Object, _collectorFactory.Object, _startingUri, _loggerMock.Object);

        _collector.Setup(c => c.Collect()).ReturnsAsync(_collectorReturnedUris);
        _collectorFactory.Setup(c => c.Create(It.IsAny<Uri>(), It.IsAny<Uri>())).Returns(_collector.Object);
    }

    [Fact]
    public void Run_EnqueuesStartingUri()
    {
        var result = _sut.RunAsync();

        _queueMock.Verify(q => q.Enqueue(It.Is<IEnumerable<Uri>>(s => s.First().AbsoluteUri == _startingUri.AbsoluteUri)));
        _queueMock.Verify(q => q.Enqueue(It.IsAny<IEnumerable<Uri>>()), Times.Once);
    }

    [Fact]
    public void Run_Dequeues()
    {
        _queueMock.Setup(q => q.Count).Returns(1);
        _queueMock.Setup(q => q.TryDequeue(out _startingUri)).Returns(() => { _queueMock.Setup(q => q.Count).Returns(0); return true; });

        var result = _sut.RunAsync();

        _queueMock.Verify(q => q.TryDequeue(out It.Ref<Uri>.IsAny));
    }

    [Fact]
    public void Run_HasEnqueued_CallsCollectOnCollector()
    {
        _queueMock.Setup(q => q.Count).Returns(1);
        _queueMock.Setup(q => q.TryDequeue(out _startingUri)).Returns(() => { _queueMock.Setup(q => q.Count).Returns(0); return true; });

        var result = _sut.RunAsync();

        _collector.Verify(c => c.Collect(), Times.Once);
    }

    [Fact]
    public void Run_HasEnqueued_EnqueuesCollectorResults()
    {
        var collectedUris = new List<Uri>() { UriHelper.GetRandomUri() };
        _queueMock.Setup(q => q.Count).Returns(1);
        _queueMock.Setup(q => q.TryDequeue(out _startingUri)).Returns(() => { _queueMock.Setup(q => q.Count).Returns(0); return true; });
        _collectorReturnedUris.AddRange(collectedUris);

        var result = _sut.RunAsync();

        _queueMock.Verify(q => q.Enqueue(It.Is<IEnumerable<Uri>>(u => u.First().AbsoluteUri == collectedUris.First().AbsoluteUri)), Times.Once);
        _queueMock.Verify(q => q.Enqueue(It.IsAny<IEnumerable<Uri>>()), Times.Exactly(2));
    }
}