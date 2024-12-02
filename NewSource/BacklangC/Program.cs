using System.Reflection;
using CommandLine;
using Parser = CommandLine.Parser;

namespace BacklangC;

public static class Program
{
    public static void Main(string[] args)
    {
        //hack
        var a = Assembly.LoadFrom("Backlang.CodeAnalysis.dll");

        Parser.Default.ParseArguments<DriverSettings>(args)
            .WithParsed(options =>
            {
                var driver = Driver.Create(options);

                driver.Compile();
            })
            .WithNotParsed(errors =>
            {
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ToString());
                }
            });
    }
}