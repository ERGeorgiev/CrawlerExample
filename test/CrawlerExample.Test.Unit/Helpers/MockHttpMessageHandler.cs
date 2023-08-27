namespace CrawlerExample.Test.Unit.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public HttpResponseMessage Response { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(Response);
    }
}