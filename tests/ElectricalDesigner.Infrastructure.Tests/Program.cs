using System.Reflection;
using ElectricalDesigner.TestKit;

Console.WriteLine("=== ElectricalDesigner.Infrastructure.Tests ===");
Console.WriteLine();

return TestRunner.RunAll(Assembly.GetExecutingAssembly());
