using CrawlerExample.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links
{
    public interface IConcurrentUniqueUriQueue
    {
        int Count { get; }
        int HistoricalCount { get; }

        void Enqueue(IEnumerable<Uri> uris);
        IEnumerable<Uri> GetHistoricalEnqueued();
        bool TryDequeue([MaybeNullWhen(false)] out Uri result);
    }
}