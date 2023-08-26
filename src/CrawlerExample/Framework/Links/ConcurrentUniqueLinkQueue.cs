using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links;

public class ConcurrentUniqueLinkQueue : ILinkQueue
{
    private readonly object _linksLock = new();
    private readonly Queue<Uri> _links = new();
    private readonly HashSet<Uri> _uniqueLinks = new();

    public int TotalUniqueEnqueued => _uniqueLinks.Count;

    public IEnumerable<Uri> GetUniqueEnqueued()
    {
        lock (_linksLock)
        {
            return _uniqueLinks.ToArray();
        }
    }

    public void Enqueue(IEnumerable<Uri> uris)
    {
        lock (_linksLock)
        {
            foreach (var uri in uris)
            {
                if (_uniqueLinks.Add(uri)) _links.Enqueue(uri);
            }
        }
    }

    public bool TryDequeue([MaybeNullWhen(false)] out Uri result)
    {
        lock (_linksLock)
        {
            return _links.TryDequeue(out result);
        }
    }

    public int Count => _links.Count;
}