using Backlang.Driver.Compiling.Stages;

namespace Backlang.Driver;

public class CompilerDriver
{
    public static async void Compile(CompilerContext context)
    {
        var pipeline = Flo.Pipeline.Build<CompilerContext, CompilerContext>(
       cfg => {
           cfg.Add<ParsingStage>();
           cfg.Add<InitTypeSystemStage>();

           cfg.When(_ => _.References.Any(), _ => {
               _.Add<InitReferencesStage>();
           });

           cfg.Add<LowererStage>();
           cfg.Add<ExpandImplementationStage>();
           cfg.Add<ImplementationStage>();
           cfg.Add<ExpandMacrosStage>();
           cfg.Add<IntermediateStage>();
       }
       );

        await pipeline.Invoke(context);
    }
}