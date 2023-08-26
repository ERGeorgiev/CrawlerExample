using CrawlerExample.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links;

public class ConcurrentUniqueUriQueue
{
    private readonly object _urisLock = new();
    private readonly Queue<Uri> _uris = new();
    private readonly HashSet<string> _historicalUris = new(StringComparer.OrdinalIgnoreCase);

    private List<Uri> _ignoredBaseUris = new();

    public int HistoricalCount => _historicalUris.Count;

    public int Count => _uris.Count;

    public void Configure(RobotsFileConfiguration robotsFileConfiguration)
    {
        lock (_urisLock)
        {
            _ignoredBaseUris = robotsFileConfiguration.Disallow.Select(d => new Uri(d.OriginalString, UriKind.Relative)).ToList();
        }
    }

    public IEnumerable<Uri> GetHistoricalEnqueued()
    {
        lock (_urisLock)
        {
            return _historicalUris.Select(u => new Uri(u)).ToArray();
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
                if (_historicalUris.Add(uri.AbsoluteUri) && UriPointsToMediaFile(uri) == false)
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

    private static bool UriPointsToMediaFile(Uri uri)
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