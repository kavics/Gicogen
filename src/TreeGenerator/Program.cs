using System.Diagnostics;
using System.Net.WebSockets;
using TreeManager;
/*
var maxCount = 100;

var count = 0;
foreach (var tNode in TreeGenerator.GenerateTree(0))
{
    Console.Write($"{tNode.NodeId}:{tNode.PathToken} ");
    if (++count >= maxCount)
        break;
}
Console.WriteLine();
Console.WriteLine();

count = 0;
foreach (var tNode in TreeGenerator.GenerateTree(1_222_222_220L))
{
    Console.Write($"{tNode.NodeId}:{tNode.PathToken} ");
    if (++count >= maxCount)
        break;
}
Console.WriteLine();
Console.WriteLine();

Console.WriteLine($"MaxInt: {int.MaxValue}");
Console.WriteLine($"MaxId:  {1_222_222_220}");
Console.WriteLine($"Id to token:  1222222220 --> {TreeGenerator.IdToToken(1_222_222_220)}");
Console.WriteLine($"Token to Id:  RJJJJJJJJj --> {TreeGenerator.TokenToId("RJJJJJJJJj")}");
Console.WriteLine($"Token to Id:  RJJJJJJJj  --> {TreeGenerator.TokenToId("RJJJJJJJj")}");
Console.WriteLine($"Token to Id:  RJJJJJJj   --> {TreeGenerator.TokenToId("RJJJJJJj")}");
Console.WriteLine($"Token to Id:  RJJJJJj    --> {TreeGenerator.TokenToId("RJJJJJj")}");
Console.WriteLine($"Token to Id:  RJJJJj     --> {TreeGenerator.TokenToId("RJJJJj")}");
Console.WriteLine($"Token to Id:  RJJJj      --> {TreeGenerator.TokenToId("RJJJj")}");
Console.WriteLine($"Token to Id:  RJJj       --> {TreeGenerator.TokenToId("RJJj")}");
Console.WriteLine($"Token to Id:  RJj        --> {TreeGenerator.TokenToId("RJj")}");
Console.WriteLine();
Console.WriteLine($"Id to token:  23221 --> {TreeGenerator.IdToToken(23221)}");


Console.WriteLine($"Token to Id:  RAAAAa     --> {TreeGenerator.TokenToId("RAAAAa")}");
Console.WriteLine($"Token to Id:  RAJJJj     --> {TreeGenerator.TokenToId("RAJJJj")}");
Console.WriteLine($"Token to Id:  RBAAA      --> {TreeGenerator.TokenToId("RBAAA")}");
Console.WriteLine($"Token to Id:  RBAAAa     --> {TreeGenerator.TokenToId("RBAAAa")}");

Console.WriteLine($"Token to Id:  RCAAAa     --> {TreeGenerator.TokenToId("RCAAAa")}");
Console.WriteLine($"Token to Id:  RDAAAa     --> {TreeGenerator.TokenToId("RDAAAa")}");
Console.WriteLine($"Token to Id:  REAAAa     --> {TreeGenerator.TokenToId("REAAAa")}");
Console.WriteLine($"Token to Id:  RFAAAa     --> {TreeGenerator.TokenToId("RFAAAa")}");
Console.WriteLine($"Token to Id:  RGAAAa     --> {TreeGenerator.TokenToId("RGAAAa")}");
Console.WriteLine($"Token to Id:  RHAAAa     --> {TreeGenerator.TokenToId("RHAAAa")}");
Console.WriteLine($"Token to Id:  RIAAAa     --> {TreeGenerator.TokenToId("RJAAAa")}");
Console.WriteLine($"Token to Id:  RJAAAa     --> {TreeGenerator.TokenToId("RJAAAa")}");
Console.WriteLine($"Token to Id:  RJJJJj     --> {TreeGenerator.TokenToId("RJJJJj")}");
Console.WriteLine($"Token to Id:  RJJJJJ     --> {TreeGenerator.TokenToId("RJJJJj")}");
*/
var count = 0;
Console.WriteLine($"Start.");
var timer = Stopwatch.StartNew();
var generator = new AsyncTreeGenerator(10, 6);
await generator.GenerateAsync(async (path, taskCount) =>
{
    Interlocked.Increment(ref count);
    Console.Write($"O");
    //Console.Write($"{path} ");
    //Console.Write($"{taskCount} ");
    await Task.Delay(1);
});
timer.Stop();
Console.WriteLine();
Console.WriteLine($"Done. {count} nodes generated in {timer.Elapsed}. Max task count: {generator.MaxTaskCount}");
