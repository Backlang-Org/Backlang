﻿using Backlang_Compiler.Compiling.Stages;
using CommandLine;

namespace Backlang_Compiler;

public static class Program
{
    public static void Main(string[] args)
    {
        var pipeline = Flo.Pipeline.Build<CompilerContext, CompilerContext>(
       cfg => {
           cfg.Add<ParsingStage>();
           cfg.Add<LowererStage>();
           cfg.Add<OptimizingStage>();
           cfg.Add<EmitStage>();
       }
   );

        Parser.Default.ParseArguments<CompilerContext>(args)
              .WithParsed(async o => {
                  _ = await pipeline.Invoke(o);
              });
    }
}