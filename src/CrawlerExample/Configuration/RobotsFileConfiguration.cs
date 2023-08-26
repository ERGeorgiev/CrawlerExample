namespace CrawlerExample.Configuration;

public class RobotsFileConfiguration
{
    public int CrawlDelay { get; set; } = 0;

    public List<Uri> Disallow = new();
}