﻿using CrawlerExample.Configuration;
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
        Console.WriteLine("Crawl Uri: {0}", startingUri);

        var config = GetCrawlerExampleConfiguration();
        var crawler = new Crawler(config, startingUri, GetLoggerFactory());

        Console.WriteLine("Starting Crawler...");
        var crawlerTask = crawler.Run();
        Console.WriteLine("Crawler Started Successfully");
        while (crawlerTask.Status == TaskStatus.Running || crawlerTask.Status == TaskStatus.WaitingForActivation)
        {
            Console.WriteLine("Number of uris found: {0}", crawler.Count);
            Task.Delay(1000).Wait();
        }

        Console.WriteLine();
        Console.WriteLine("{0} unique uris found:", crawler.Count);
        foreach (var uri in crawler.Results.OrderBy(r => r.AbsoluteUri))
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
