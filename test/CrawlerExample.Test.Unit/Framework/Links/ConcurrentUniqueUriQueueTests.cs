using CrawlerExample.Configuration;
using CrawlerExample.Framework.Links;
using System.Net.Http.Headers;

namespace CrawlerExample.Test.Unit;

public class ConcurrentUniqueUriQueueTests
{
    private readonly ConcurrentUniqueUriQueue _sut = new();

    [Fact]
    public void Enqueue_CountIncreased()
    {
        var exampleUriA = GetRandomUri();
        var expectedCount = _sut.Count + 1;

        _sut.Enqueue(new Uri[] { exampleUriA });

        Assert.Equal(expectedCount, _sut.Count);
    }

    [Fact]
    public void Enqueue_Duplicate_CountNotIncreased()
    {
        var exampleUriA = GetRandomUri();
        var exampleUriB = new Uri(exampleUriA.OriginalString);
        var expectedCount = _sut.Count + 1;

        _sut.Enqueue(new Uri[] { exampleUriA, exampleUriB });

        Assert.Equal(expectedCount, _sut.Count);
    }

    [Fact]
    public void Enqueue_MediaUri_OnlyHistoricalCountIncreased()
    {
        var exampleUriA = GetRandomMediaUri();
        var expectedCount = _sut.Count;
        var expectedHistoricalCount = _sut.HistoricalCount + 1;

        _sut.Enqueue(new Uri[] { exampleUriA });

        Assert.Equal(expectedCount, _sut.Count);
        Assert.Equal(expectedHistoricalCount, _sut.HistoricalCount);
    }

    [Fact]
    public void Enqueue_HistoricalCountIncreased()
    {
        var exampleUriA = GetRandomUri();
        var expectedCount = _sut.HistoricalCount + 1;

        _sut.Enqueue(new Uri[] { exampleUriA });

        Assert.Equal(expectedCount, _sut.HistoricalCount);
    }

    [Fact]
    public void Enqueue_Duplicate_HistoricalCountNotIncreased()
    {
        var exampleUriA = GetRandomUri();
        var exampleUriB = new Uri(exampleUriA.OriginalString);
        var expectedCount = _sut.HistoricalCount + 1;

        _sut.Enqueue(new Uri[] { exampleUriA, exampleUriB });

        Assert.Equal(expectedCount, _sut.HistoricalCount);
    }

    [Fact]
    public void Enqueue_Duplicate_GetEnqueuedMatchesUniqueInput()
    {
        var exampleUriA = GetRandomUri();
        var exampleUriB = new Uri(exampleUriA.OriginalString);

        _sut.Enqueue(new Uri[] { exampleUriA, exampleUriB });

        Assert.Single(_sut.GetHistoricalEnqueued());
        Assert.Equal(exampleUriA.AbsoluteUri, _sut.GetHistoricalEnqueued().First().AbsoluteUri);
    }

    [Fact]
    public void Enqueue_ConfiguredToIgnore_NotEnqueued()
    {
        var exampleUriA = new Uri(GetRandomUri().AbsoluteUri + "ignored/", UriKind.Absolute);
        var config = new RobotsFileConfiguration()
        {
            Disallow = new List<Uri>()
            {
                new Uri("/ignored/", UriKind.Relative)
            }
        };

        _sut.Configure(config);
        _sut.Enqueue(new Uri[] { exampleUriA });

        Assert.Empty(_sut.GetHistoricalEnqueued());
    }

    [Fact]
    public void Dequeue_TrueAndSameAsEnqueued()
    {
        var exampleUriA = GetRandomUri();
        _sut.Enqueue(new Uri[] { exampleUriA });

        var result = _sut.TryDequeue(out var uriResult);

        Assert.True(result);
        Assert.Equal(exampleUriA.AbsoluteUri, uriResult.AbsoluteUri);
    }

    [Fact]
    public void Dequeue_CountIsLess()
    {
        var exampleUriA = GetRandomUri();
        _sut.Enqueue(new Uri[] { exampleUriA });
        var expectedCount = _sut.Count - 1;

        _sut.TryDequeue(out var _);

        Assert.Equal(expectedCount, _sut.Count);
    }

    [Fact]
    public void Dequeue_HistoricalCountIsMore()
    {
        var exampleUriA = GetRandomUri();
        var expectedHistoricalCount = _sut.Count + 1;
        _sut.Enqueue(new Uri[] { exampleUriA });

        _sut.TryDequeue(out var _);

        Assert.Equal(expectedHistoricalCount, _sut.HistoricalCount);
    }

    [Fact]
    public void Dequeue_EmptyQueue_ReturnFalse()
    {
        var result = _sut.TryDequeue(out var _);

        Assert.False(result);
    }

    private static Uri GetRandomUri() => new($"https://www.{Random.Shared.Next()}.com");

    private static Uri GetRandomMediaUri() => new($"https://www.{Random.Shared.Next()}.com/hi.jpg");
}