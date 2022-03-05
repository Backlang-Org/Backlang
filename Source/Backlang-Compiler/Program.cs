using CommandLine;
using Backlang_Compiler.Compiling.Stages;
using Backlang_Compiler;

namespace Backlang_Compiler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var pipeline = Flo.Pipeline.Build<CompilerContext, CompilerContext>(
           cfg =>
           {
               cfg.Add<ParsingStage>();

               cfg.Add<OptimizingStage>();
           }
       );

            Parser.Default.ParseArguments<CompilerContext>(Environment.GetCommandLineArgs())
                  .WithParsed(async o =>
                  {
                      _ = await pipeline.Invoke(o);
                  });
        }
    }
}
