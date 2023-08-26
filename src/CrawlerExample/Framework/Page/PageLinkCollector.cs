using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CrawlerExample.Framework.Page;

public class PageLinkCollector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PageLinkCollector> _logger;

    public PageLinkCollector(HttpClient httpClient, Uri baseUri, Uri collectionUri, ILogger<PageLinkCollector> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        BaseUri = baseUri;
        CollectionUri = collectionUri;
    }

    public Uri BaseUri { get; set; }

    public Uri CollectionUri { get; set; }

    public async Task<IEnumerable<Uri>> Extract()
    {
        var regexBase = BaseUri.AbsoluteUri.ToString();
        regexBase = regexBase.Replace("https://", "");
        regexBase = regexBase.Replace("http://", "");
        regexBase = regexBase.Replace("www.", "");
        regexBase = regexBase.Replace(@"\", @"\\");
        regexBase = regexBase.Replace(@"/", "\\/");
        regexBase = regexBase.Replace(@".", "\\.");
        var regex = new Regex(@$"(?<= |{"\""})(http:\/\/|https:\/\/|a^)(www\.){regexBase}[-a-zA-Z0-9()@:%_\+.~#?&\/=;]*");
        var uris = new List<Uri>();

        using HttpResponseMessage response = await _httpClient.GetAsync(CollectionUri);
        using HttpContent content = response.Content;
        using Stream stream = await content.ReadAsStreamAsync();
        using StreamReader streamReader = new(stream);
        {
            string result = await content.ReadAsStringAsync();
            var matches = regex.Matches(result);
            uris.AddRange(matches.Select(m => new Uri(m.Value)));
        }

        _logger.LogInformation("Collected Uri: {uri}, Uris found:\n{uris}", CollectionUri.AbsoluteUri, string.Join('\n', uris.Select(u => u.AbsoluteUri)));
        return uris;
    }
}