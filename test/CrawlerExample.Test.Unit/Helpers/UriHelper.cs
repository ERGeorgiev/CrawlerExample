namespace CrawlerExample.Test.Unit.Helpers;

public static class UriHelper
{
    public static Uri GetRandomUri() => new($"https://www.{Random.Shared.Next()}.com");
    public static Uri GetRandomMediaUri() => new($"https://www.{Random.Shared.Next()}.com/hi.jpg");
}