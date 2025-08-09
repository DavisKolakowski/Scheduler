using NodaTime;
using Scheduler.Core.Enums;
using Scheduler.Core;
using System.Text.Json;

namespace Scheduler.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Testing New Schedule System with JSON Output ---");
        Console.WriteLine();

        ScheduleDemos.RunAll();

        Console.WriteLine("--- End of Tests ---");
    }
}
