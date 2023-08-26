namespace CrawlerExample.Framework.Page;

public class PageLinkExtractor
{
    public PageLinkExtractor(Uri uri)
    {
        Uri = uri;
    }

    public Uri Uri { get; set; }

    public async Task<IEnumerable<Uri>> Extract()
    {
        await Task.Delay(1000);
        return new List<Uri>() { new Uri("https://www.youtube.com/1"), new Uri("https://www.youtube.com/2") };
    }
}