namespace CrawlerExample.Configuration;

public class RobotsFileConfiguration
{
    public int CrawlDelay { get; set; } = 2500;

    public List<Uri> Disallow = new();
}