namespace TreeManager;

public class AsyncTreeGenerator
{
    public int _containersPerLevel; // max 26;
    public int _levelMax;
    private int _taskCount;
    private int _maxTaskCount;

    public int MaxTaskCount => _maxTaskCount;

    public AsyncTreeGenerator(int containersPerLevel = 10, int levelMax = 9)
    {
        _containersPerLevel = containersPerLevel;
        _levelMax = levelMax;
    }

    public Task GenerateAsync(Func<string, int, Task> createNodeCallback)
    {
        return CreateNodeAsync2(createNodeCallback, "R");
    }

    //private async Task CreateNodeAsync(Func<string, int, Task> createNodeCallback, string pathToken)
    //{
    //    await createNodeCallback(pathToken, _taskCount);
    //    if (pathToken.Length >= _levelMax)
    //        return;
    //    var tasks = new Task[_containersPerLevel];
    //    for (var i = 0; i < _containersPerLevel; i++)
    //    {
    //        var token = pathToken + (char)('0' + i);
    //        Interlocked.Increment(ref _taskCount);
    //        tasks[i] = CreateNodeAsync(createNodeCallback, token);
    //    }
    //    await Task.WhenAll(tasks);
    //    Console.Write(".");
    //    Interlocked.Add(ref _taskCount, -10);
    //}
    private async Task CreateNodeAsync2(Func<string, int, Task> createNodeCallback, string pathToken)
    {
        await createNodeCallback(pathToken, _taskCount);
        if (pathToken.Length >= _levelMax)
            return;

        if (pathToken.Length + 1 >= _levelMax)
        {
            for (var i = 0; i < _containersPerLevel; i++)
            {
                var token = pathToken + (char)('0' + i);
                var taskCount = Interlocked.Increment(ref _taskCount);
                if (taskCount > _maxTaskCount)
                    Interlocked.Exchange(ref _maxTaskCount, taskCount);

#pragma warning disable CS4014
                CreateNodeAsync2(createNodeCallback, token).ContinueWith(task =>
#pragma warning restore CS4014
                {
                    Console.Write(".");
                    Interlocked.Decrement(ref _taskCount);
                });
            }
        }
        else
        {
            var tasks = new Task[_containersPerLevel];
            for (var i = 0; i < _containersPerLevel; i++)
            {
                var token = pathToken + (char)('0' + i);
                Interlocked.Increment(ref _taskCount);
                tasks[i] = CreateNodeAsync2(createNodeCallback, token);
            }
            await Task.WhenAll(tasks);
            Console.Write("-");
            Interlocked.Add(ref _taskCount, -10);
        }
    }
}