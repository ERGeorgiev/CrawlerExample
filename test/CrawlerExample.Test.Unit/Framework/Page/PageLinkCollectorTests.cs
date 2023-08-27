using CrawlerExample.Framework.Page;
using Microsoft.Extensions.Logging;

namespace CrawlerExample.Test.Unit;

public class PageLinkCollectorTests
{
    private readonly HttpClient _httpClient;
    private readonly MockHttpMessageHandler _mockHttpMessageHandler = new();
    private readonly Mock<ILogger<PageLinkCollector>> _loggerMock = new();
    private readonly Uri _baseUri;
    private readonly Uri _collectionUri;
    private readonly PageLinkCollector _sut;

    public PageLinkCollectorTests()
    {
        _baseUri = UriHelper.GetRandomUri();
        _collectionUri = new Uri(_baseUri.AbsoluteUri + "test/test2/test3", UriKind.Absolute); 
        _httpClient = new HttpClient(_mockHttpMessageHandler);
        _sut = new(_httpClient, _baseUri, _collectionUri, _loggerMock.Object);
    }

    [Fact]
    public void Collect_UriSurroundedBySpaces_UriCollected()
    {
        var hiddenUri = new Uri(_baseUri.AbsoluteUri + "test1/test2", UriKind.Absolute);
        var content = $"Hello world, this is my uri: \" {hiddenUri} \".";
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(content)
        };
        _mockHttpMessageHandler.Response = response;

        var result = _sut.Collect().Result;

        Assert.Equal(result.First().AbsoluteUri, hiddenUri.AbsoluteUri);
    }

    [Fact]
    public void Collect_UriSurroundedByQuotes_UriCollected()
    {
        var hiddenUri = new Uri(_baseUri.AbsoluteUri + "test1/test2", UriKind.Absolute);
        var content = @$"Hello world, this is my uri: ""{hiddenUri}"".";
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(content)
        };
        _mockHttpMessageHandler.Response = response;

        var result = _sut.Collect().Result;

        Assert.Equal(result.First().AbsoluteUri, hiddenUri.AbsoluteUri);
    }

    [Fact]
    public void Collect_UriDoesNotContainBaseUri_NoneCollected()
    {
        var hiddenUri = UriHelper.GetRandomUri();
        var content = @$"Hello world, this is my uri: ""{hiddenUri}"".";
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(content)
        };
        _mockHttpMessageHandler.Response = response;

        var result = _sut.Collect().Result;

        Assert.Empty(result);
    }
}