using BenchmarkDotNet.Running;

namespace Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        return;

        var b = new ValidationBenchmarks();
        b.Setup();
        Console.WriteLine("press key");
        Console.ReadKey();
        for (int i = 0; i < 1000; i++)
        {
            var result = b.Validate_Person();
        }
    }
}