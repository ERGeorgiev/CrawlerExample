namespace CrawlerExample.Framework.Links;

public class ConcurrentLinkQueue
{
    private readonly object _linksLock = new();
    private readonly Queue<Uri> _links = new();

    public void Enqueue(IEnumerable<Uri> uris)
    {
        lock (_linksLock)
        {
            foreach (var uri in uris)
            {
                _links.Enqueue(uri);
            }
        }
    }

    public bool TryDequeue(out Uri? result)
    {
        lock (_linksLock)
        {
            return _links.TryDequeue(out result);
        }
    }
}