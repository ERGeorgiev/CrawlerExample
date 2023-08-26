namespace CrawlerExample.Configuration;

public class CrawlerConfiguration
{
    public const string Section = "Crawler";

    public int ThreadsMax { get; set; } = 10;
}