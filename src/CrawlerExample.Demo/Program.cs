using CrawlerExample.Configuration;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace CrawlerExample.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("CrawlerExample Demo v{0}", Assembly.GetExecutingAssembly().GetName().Version!.ToString());
        Console.WriteLine();

        var startingLink = GetLinkFromArgs(args);
        Console.WriteLine("Crawl Link: {0}", startingLink);

        var config = GetCrawlerExampleConfiguration();
        var crawler = new Crawler(config);

        Console.WriteLine("Starting Crawler...", startingLink);
        var crawlerTask = crawler.Run(startingLink);
        while (crawlerTask.Status == TaskStatus.WaitingForActivation)
        {
            Task.Delay(10).Wait();
        }
        Console.WriteLine("Crawler Started Successfully", startingLink);
        while (crawlerTask.Status == TaskStatus.Running)
        {
            Console.WriteLine("Number of links found: {0}", crawler.Count);
            Task.Delay(1000).Wait();
        }

        Console.WriteLine();
        Console.WriteLine("All links found:", crawler.Count);
        foreach (var uri in crawler.Results)
        {
            Console.WriteLine(uri.AbsoluteUri);
        }
    }

    private static Uri GetLinkFromArgs(string[] args)
    {
        if (args.Any() == false) throw new ArgumentException("Please provide a link in the arguments.");

        var link = args.First();
        var uri = new Uri(link);

        return uri;
    }

    private static CrawlerConfiguration GetCrawlerExampleConfiguration()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
        var crawlerExampleConfig = config.GetSection(CrawlerConfiguration.Section).Get<CrawlerConfiguration>();
        if (crawlerExampleConfig == null)
        {
            throw new NullReferenceException("Unable to read configuration.");
        }

        return crawlerExampleConfig;
    }
}
