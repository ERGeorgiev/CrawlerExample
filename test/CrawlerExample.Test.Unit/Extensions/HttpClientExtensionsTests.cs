using CrawlerExample.Extensions;

namespace CrawlerExample.Test.Unit.Extensions;

public class HttpClientExtensionsTests
{
    private readonly HttpClient _sut = new();

    [Fact]
    public void AddUserAgent_AddsCorrectUserAgent()
    {
        var expectedUserAgent = @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11";

        _sut.AddUserAgent();

        Assert.Equal(expectedUserAgent, string.Join(" ", _sut.DefaultRequestHeaders.GetValues("User-Agent")));
    }

    [Fact]
    public void AddUserAgent_DoesNotAddUnexpectedHeaders()
    {
        var expectedHeaderCount = _sut.DefaultRequestHeaders.Count() + 1;

        _sut.AddUserAgent();

        Assert.Equal(expectedHeaderCount, _sut.DefaultRequestHeaders.Count());
    }
}