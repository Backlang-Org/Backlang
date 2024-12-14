using CommandLine;

namespace BacklangC;

public static class Program
{
    public static void Main(string[] args)
    {
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