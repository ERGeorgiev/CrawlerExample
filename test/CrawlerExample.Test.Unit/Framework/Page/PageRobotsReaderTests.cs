using CrawlerExample.Framework.Page;
using Microsoft.Extensions.Logging;

namespace CrawlerExample.Test.Unit.Framework.Page;

public class PageRobotsReaderTests
{
    private readonly HttpClient _httpClient;
    private readonly MockHttpMessageHandler _mockHttpMessageHandler = new();
    private readonly Mock<ILogger<PageRobotsReader>> _loggerMock = new();
    private readonly Uri _baseUri;
    private readonly PageRobotsReader _sut;

    public PageRobotsReaderTests()
    {
        _baseUri = UriHelper.GetRandomUri();
        _httpClient = new HttpClient(_mockHttpMessageHandler);
        _sut = new(_httpClient, _baseUri, _loggerMock.Object);
    }

    [Fact]
    public void Get_LoadsTheCorrectDisallow()
    {
        var content = $"User-agent: *\nDisallow: /test/\nDisallow: /test2/\n";
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(content)
        };
        _mockHttpMessageHandler.Response = response;

        var result = _sut.Get().Result;

        Assert.Equal(2, result.Disallow.Count);
        Assert.Equal("/test/", result.Disallow.First().OriginalString);
        Assert.Equal("/test2/", result.Disallow.ElementAt(1).OriginalString);
    }

    [Fact]
    public void Get_LoadsTheCorrectCrawlDelay()
    {
        var content = $"User-agent: *\nDisallow: /test/\nDisallow: /test2/\nCrawl-delay: 4\n";
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(content)
        };
        _mockHttpMessageHandler.Response = response;

        var result = _sut.Get().Result;

        Assert.Equal(4, result.CrawlDelay);
    }

    [Fact]
    public void Get_TwoAgents_LoadsTheCorrectOne()
    {
        var content = $"User-agent: test\nDisallow: /wrong/\nUser-agent: *\nDisallow: /correct/\n";
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(content)
        };
        _mockHttpMessageHandler.Response = response;

        var result = _sut.Get().Result;

        Assert.Single(result.Disallow);
        Assert.Equal("/correct/", result.Disallow.First().OriginalString);
    }
}