using Looplet.Jobs;

namespace Looplet.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Console.WriteLine(typeof(SampleConsoleJob).FullName);
        Console.WriteLine(typeof(SampleConsoleJob).Assembly.GetName().Name);
    }
}