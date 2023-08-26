using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links;

public static class HttpClientExtensions
{
    private readonly static string _headerUserAgent = 
        @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11";

    public static void AddUserAgent(this HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Add("User-Agent", _headerUserAgent);
    }
}