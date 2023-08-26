namespace CrawlerExample.Framework.Links;

public class ConcurrentLinkStorage
{
    private readonly object _linksLock = new();
    private readonly HashSet<Uri> _links = new();

    public void Add(IEnumerable<Uri> uris)
    {
        lock (_linksLock)
        {
            foreach (var uri in uris)
            {
                _links.Add(uri);
            }
        }
    }

    public bool Contains(Uri uri)
    {
        lock (_linksLock)
        {
            return _links.Contains(uri);
        }
    }

    public IEnumerable<Uri> GetCopy()
    {
        lock (_linksLock)
        {
            return _links.ToArray();
        }
    }
}