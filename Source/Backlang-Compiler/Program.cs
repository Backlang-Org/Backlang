using Backlang.Driver;
using CommandLine;

namespace Backlang_Compiler;

public static class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<CompilerContext>(args)
              .WithParsed(context => {
                  CompilerDriver.Compile(context);
              });
    }
}