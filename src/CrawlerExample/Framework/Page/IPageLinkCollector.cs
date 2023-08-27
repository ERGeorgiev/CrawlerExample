namespace CrawlerExample.Framework.Page
{
    public interface IPageLinkCollector
    {
        Uri BaseUri { get; set; }
        Uri CollectionUri { get; set; }

        Task<IEnumerable<Uri>> Collect();
    }
}