using CrawlerExample.Configuration;
using CrawlerExample.Framework.Links;
using CrawlerExample.Framework.Page;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CrawlerExample.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("CrawlerExample Demo v{0}", Assembly.GetExecutingAssembly().GetName().Version!.ToString());
        Console.WriteLine();

        var startingUri = GetLinkFromArgs(args);
        var httpClient = new HttpClient();
        var loggerFactory = GetLoggerFactory();
        var config = GetCrawlerExampleConfiguration();
        var robotsConfig = new PageRobotsReader(httpClient, startingUri, loggerFactory.CreateLogger<PageRobotsReader>()).Get().Result;
        var queue = new ConcurrentUniqueUriQueue(robotsConfig);
        var collectorFactory = new PageLinkCollectorFactory(httpClient, loggerFactory);
        Console.WriteLine("Crawl Uri: {0}", startingUri);

        var crawler = new Crawler(config, httpClient, queue, collectorFactory, startingUri, loggerFactory);
        Console.WriteLine("Starting Crawler...");
        var crawlerTask = crawler.Run();
        Console.WriteLine("Crawler Started Successfully");
        while (crawlerTask.Status == TaskStatus.Running || crawlerTask.Status == TaskStatus.WaitingForActivation)
        {
            Console.WriteLine("Number of uris found: {0}", queue.HistoricalCount);
            Task.Delay(1000).Wait();
        }

        if (crawlerTask.IsFaulted)
        {
            if (crawlerTask.Exception != null)
            {
                throw crawlerTask.Exception;
            }
            else
            {
                throw new Exception("Unknown error.");
            }
        }

        Console.WriteLine();
        Console.WriteLine("{0} unique uris found:", queue.HistoricalCount);
        foreach (var uri in queue.GetHistoricalEnqueued().OrderBy(r => r.AbsoluteUri))
        {
            Console.WriteLine(uri.AbsoluteUri);
        }
    }

    private static ILoggerFactory GetLoggerFactory()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        return loggerFactory;
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
