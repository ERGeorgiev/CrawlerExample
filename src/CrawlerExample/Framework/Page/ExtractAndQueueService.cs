using CrawlerExample.Framework.Links;

namespace CrawlerExample.Framework.Page;

public class ExtractAndQueueService
{
    private readonly ILinkQueue _linkQueue;
    private readonly object _lock = new();
    private int _threadsRunning = 0;
    private SemaphoreSlim _semaphoreSlim;

    public ExtractAndQueueService(ILinkQueue linkQueue, int threadsMax)
    {
        _semaphoreSlim = new(threadsMax, threadsMax);
        _linkQueue = linkQueue;
        ThreadsMax = threadsMax;
    }

    public int ThreadsMax { get; }

    public bool Running => _threadsRunning > 0;

    public bool BeginExtraction()
    {
        lock (_lock)
        {
            if (_threadsRunning >= ThreadsMax) return false;

            if (_linkQueue.TryDequeue(out Uri? uri))
            {
                var pageLinkExtractor = new PageLinkExtractor(uri);
                _threadsRunning++;
                var task = ExtractAndEnqueue(pageLinkExtractor);
                task.Start();

                return true;
            }

            return false;
        }
    }

    private async Task ExtractAndEnqueue(PageLinkExtractor extractor)
    {
        try
        {
            throw new NullReferenceException();
            var results = await extractor.Extract();
            _linkQueue.Enqueue(results);
        }
        catch (OperationCanceledException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw;
        }
        finally
        {
            lock (_lock)
            {
                _threadsRunning--;
            }
        }
    }
}