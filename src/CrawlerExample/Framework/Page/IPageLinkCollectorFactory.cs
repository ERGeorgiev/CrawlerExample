namespace CrawlerExample.Framework.Page
{
    public interface IPageLinkCollectorFactory
    {
        IPageLinkCollector Create(Uri baseUri, Uri collectionUri);
    }
}