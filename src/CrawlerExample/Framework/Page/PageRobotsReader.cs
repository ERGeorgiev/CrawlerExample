using CrawlerExample.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CrawlerExample.Framework.Page;

public class PageRobotsReader
{
    private const string _robotsAddress = "robots.txt";
    private const string _targetSectionTitle = "User-agent: *";

    private readonly HttpClient _httpClient;
    private readonly ILogger<PageRobotsReader> _logger;

    public PageRobotsReader(HttpClient httpClient, Uri uri, ILogger<PageRobotsReader> logger)
    {
        _httpClient = httpClient;
        Uri = new Uri(uri, _robotsAddress);
        _logger = logger;
    }

    public Uri Uri { get; set; }

    public async Task<RobotsFileConfiguration> Get()
    {
        RobotsFileConfiguration config = new();

        using HttpResponseMessage response = await _httpClient.GetAsync(Uri);
        using HttpContent content = response.Content;
        using Stream stream = await content.ReadAsStreamAsync();
        using StreamReader streamReader = new(stream);
        {
            string? line = await streamReader.ReadLineAsync();
            while (line != null && line.Equals(_targetSectionTitle, StringComparison.OrdinalIgnoreCase) == false)
            {
                line = await streamReader.ReadLineAsync();
            }

            if (line != null && line.Equals(_targetSectionTitle, StringComparison.OrdinalIgnoreCase))
            {
                line = await streamReader.ReadLineAsync();
                while (line != null)
                {
                    ParseLineIntoConfig(config, line);
                    line = await streamReader.ReadLineAsync();
                }
            }
        }

        _logger.LogInformation("Robots Config: {config}", JsonConvert.SerializeObject(config, Formatting.Indented));

        return config;
    }

    private static void ParseLineIntoConfig(RobotsFileConfiguration config, string line)
    {
        var disallowField = "Disallow: ";
        if (line.StartsWith(disallowField, StringComparison.OrdinalIgnoreCase))
        {
            var disallowedRelativeUriString = line.Substring(disallowField.Length);
            var disallowedRelativeUri = new Uri(disallowedRelativeUriString, UriKind.Relative);
            config.Disallow.Add(disallowedRelativeUri);
        }

        var crawlDelayField = "Crawl-delay: ";
        if (line.StartsWith(crawlDelayField, StringComparison.OrdinalIgnoreCase))
        {
            var crawlDelayString = line.Substring(crawlDelayField.Length);
            var crawlDelayInt = int.Parse(crawlDelayString);
            config.CrawlDelay = crawlDelayInt;
        }
    }
}