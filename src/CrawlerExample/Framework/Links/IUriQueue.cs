using System.Diagnostics.CodeAnalysis;

namespace CrawlerExample.Framework.Links;

public interface IUriQueue
{
    public void Enqueue(IEnumerable<Uri> uris);

    public bool TryDequeue([MaybeNullWhen(false)] out Uri? result);
}