using Backlang.Contracts;
using Backlang.Driver;
using CommandLine;

namespace Backlang_Compiler;

public static class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<CompilerCliOptions>(args)
              .WithParsed(options => {
                  var context = new CompilerContext
                  {
                      Options = options
                  };

                  CompilerDriver.Compile(context);
              });
    }
}