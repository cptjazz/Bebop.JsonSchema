using BenchmarkDotNet.Running;

namespace Benchmarks.Bebop.JsonSchema;

public class Program
{
    public static async Task Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        return;
        
        // await _Profiler();
    }

    private static async Task _Profiler()
    {
        var b = new ValidationBenchmarks();
        await b.Setup();
        Console.WriteLine("press key");
        Console.ReadKey();
        for (int i = 0; i < 1000; i++)
        {
            await b.Validate_Person();
        }
    }
}