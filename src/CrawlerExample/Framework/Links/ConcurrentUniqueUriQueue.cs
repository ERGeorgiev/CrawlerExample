using CrawlerExample.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links;

public class ConcurrentUniqueUriQueue
{
    private readonly object _urisLock = new();
    private readonly Queue<Uri> _uris = new();
    private readonly HashSet<string> _uniqueUris = new(StringComparer.OrdinalIgnoreCase);

    private List<Uri> _ignoredBaseUris = new();

    public int TotalUniqueEnqueued => _uniqueUris.Count;
    // Enqueue, check if count is increased.
    // Enqueue, ensure count is not increased if duplicate.
    // Enqueue, check if TotalUniqueEnqueued is increased
    // Enqueue, check if TotalUniqueEnqueued is not increased on duplicate
    // Add ignored base uris, Enqueue and ensure it is ignored

    public int Count => _uris.Count;

    public void Configure(RobotsFileConfiguration robotsFileConfiguration)
    {
        lock (_urisLock)
        {
            _ignoredBaseUris = robotsFileConfiguration.Disallow.Select(d => new Uri(d.OriginalString, UriKind.Relative)).ToList();
        }
    }

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
                uriIsIgnored = _ignoredBaseUris.Any(ignored => uri.AbsolutePath.StartsWith(ignored.OriginalString, StringComparison.OrdinalIgnoreCase));
                if (uriIsIgnored)
                {
                    continue;
                }

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