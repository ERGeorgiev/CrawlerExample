using CrawlerExample.Configuration;

namespace CrawlerExample.Framework.Page;

public class PageRobotsReader
{
    private const string _robotsAddress = "robots.txt";
    private const string _targetSectionTitle = "User-agent: *";

    public PageRobotsReader(Uri uri)
    {
        Uri = new Uri(uri, _robotsAddress);
    }

    public Uri Uri { get; set; }

    public async Task<RobotsFileConfiguration> Get()
    {
        var config = new RobotsFileConfiguration();

        using HttpClient client = new();
        using HttpResponseMessage response = await client.GetAsync(Uri);
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

        string result = await content.ReadAsStringAsync();

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
    }
}