using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links;

public class ConcurrentUniqueUriQueue : IUriQueue
{
    private readonly object _urisLock = new();
    private readonly Queue<Uri> _uris = new();
    private readonly HashSet<Uri> _uniqueUris = new();
    private readonly List<Uri> _ignoredBaseUris = new();

    public int TotalUniqueEnqueued => _uniqueUris.Count;

    public int Count => _uris.Count;

    public List<Uri> IgnoredBaseUris { get; set; } = new List<Uri>();

    public IEnumerable<Uri> GetUniqueEnqueued()
    {
        lock (_urisLock)
        {
            return _uniqueUris.ToArray();
        }
    }

    public void Enqueue(IEnumerable<Uri> uris)
    {
        lock (_urisLock)
        {
            bool uriIsIgnored = false;
            foreach (var uri in uris)
            {
                uriIsIgnored = _ignoredBaseUris.Any(ignored => uri.AbsolutePath.StartsWith(ignored.AbsolutePath, StringComparison.OrdinalIgnoreCase));
                if (uriIsIgnored) continue;

                if (_uniqueUris.Add(uri) && UriPointsToFile(uri) == false)
                {
                    _uris.Enqueue(uri);
                }
            }
        }
    }

    public bool TryDequeue([MaybeNullWhen(false)] out Uri result)
    {
        lock (_urisLock)
        {
            return _uris.TryDequeue(out result);
        }
    }

    private static bool UriPointsToFile(Uri uri)
    {
        return uri.Segments.Last().Contains('.', StringComparison.OrdinalIgnoreCase);
    }
}