using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links;

public class ConcurrentUniqueUriQueue
{
    private readonly object _urisLock = new();
    private readonly Queue<Uri> _uris = new();
    private readonly HashSet<string> _uniqueUris = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Uri> _ignoredBaseUris = new();

    public int TotalUniqueEnqueued => _uniqueUris.Count;

    public int Count => _uris.Count;

    public List<Uri> IgnoredBaseUris { get; set; } = new List<Uri>();

    public IEnumerable<Uri> GetUniqueEnqueued()
    {
        lock (_urisLock)
        {
            return _uniqueUris.Select(u => new Uri(u)).ToArray();
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

                var absoluteUri = uri.AbsoluteUri.TrimEnd('\\').TrimEnd('/');
                if (_uniqueUris.Add(uri.AbsoluteUri) && UriPointsToVisualFile(uri) == false)
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

    private static bool UriPointsToVisualFile(Uri uri)
    {
        var fileEndings = new string[] { ".jpg", ".png", ".svg", ".pdf", ".mp3", ".mp4", ".m4a" };
        foreach (var ending in fileEndings)
        {
            if (uri.Segments.Last().EndsWith(ending, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}