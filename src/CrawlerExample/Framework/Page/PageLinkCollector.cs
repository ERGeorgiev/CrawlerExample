using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
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
        var regexBase = GetRegexBaseAddress();
        var regexForFullUris = new Regex(
            $@"(?:(?:https?|http?):\/\/|www\.)(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[-A-Z0-9+&@#\/%=~_|$?!:,.])*(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[A-Z0-9+&@#\/%=~_|$])");
        var regexForPartialUris = new Regex(@$"(?<=href={"\""}|url{"\""}:{"\""})\/[-a-zA-Z0-9()@:%_\+.~#\/=;]+");
        var uris = new List<Uri>();

        using HttpResponseMessage response = await _httpClient.GetAsync(CollectionUri);
        using HttpContent content = response.Content;
        using Stream stream = await content.ReadAsStreamAsync();
        using StreamReader streamReader = new(stream);
        string result = await content.ReadAsStringAsync();

        var fullUriMatches = regexForFullUris.Matches(result);
        var partialUriMatches = regexForPartialUris.Matches(result);
        foreach (Match match in fullUriMatches)
        {
            if (TryGetUriFromFullUriMatch(match.Value, out var uri))
            {
                uris.Add(uri);
            }
        }
        foreach (Match match in partialUriMatches)
        {
            if (TryGetUriFromPartialUriMatch(match.Value, out var uri))
            {
                uris.Add(uri);
            }
        }
        uris = uris.DistinctBy(u => u.AbsoluteUri).ToList();
        var hostUris = uris.Where(u => u.AbsoluteUri.Contains(BaseUri.Host, StringComparison.OrdinalIgnoreCase));

        if (uris.Any())
        {
            _logger.LogInformation("Collected Uri: {uri}, uris found:\n{uris}", CollectionUri.AbsoluteUri, string.Join('\n', uris.Select(u => u.AbsoluteUri)));
        }
        else
        {
            _logger.LogInformation("Collected Uri: {uri}, no uris found.", CollectionUri.AbsoluteUri);
        }

        return hostUris;
    }

    private string GetRegexBaseAddress()
    {
        var regexBase = BaseUri.AbsoluteUri.ToString();
        regexBase = regexBase.Replace("https://", "");
        regexBase = regexBase.Replace("http://", "");
        regexBase = regexBase.Replace("www.", "");
        regexBase = regexBase.Replace(@"\", @"\\");
        regexBase = regexBase.Replace(@"/", "\\/");
        regexBase = regexBase.Replace(@".", "\\.");

        return regexBase;
    }

    private bool TryGetUriFromFullUriMatch(string value, [MaybeNullWhen(false)] out Uri uri)
    {
        var formattedUri = value.TrimEnd('\\').TrimEnd('/');
        if (formattedUri.StartsWith(BaseUri.Host, StringComparison.OrdinalIgnoreCase))
        {
            if (BaseUri.AbsoluteUri.ToString().StartsWith("https://"))
            {
                formattedUri = "https://www." + formattedUri;
            }
            else
            {
                formattedUri = "http://www." + formattedUri;
            }
        }

        try
        {
            uri = new Uri(formattedUri);
        }
        catch (Exception)
        {
            uri = null;
            return false;
        }

        return true;
    }

    private bool TryGetUriFromPartialUriMatch(string value, [MaybeNullWhen(false)] out Uri uri)
    {
        var formattedUri = value.TrimEnd('\\').TrimEnd('/');

        try
        {
            uri = new Uri(BaseUri, formattedUri);
        }
        catch (Exception)
        {
            uri = null;
            return false;
        }

        return true;
    }
}