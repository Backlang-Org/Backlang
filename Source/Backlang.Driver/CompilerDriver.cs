using Backlang.Driver.Compiling.Stages;

namespace Backlang.Driver;

public class CompilerDriver
{
    public static async void Compile(CompilerContext context)
    {
        if (string.IsNullOrEmpty(context.TempOutputPath)) context.TempOutputPath = Environment.CurrentDirectory;

        var pipeline = Flo.Pipeline.Build<CompilerContext, CompilerContext>(
       cfg => {
           cfg.Add<ParsingStage>();
           cfg.Add<InitTypeSystemStage>();

           cfg.When(_ => _.References.Any(), _ => {
               _.Add<InitReferencesStage>();
           });

           cfg.Add<ExpandMacrosStage>();
           cfg.Add<IntermediateStage>();
           cfg.Add<ExpandImplementationStage>();
           cfg.Add<ImplementationStage>();

           cfg.When(_ => _.OutputTree, _ => {
               _.Add<EmitTreeStage>();
           });
       }
       );

        await pipeline.Invoke(context);
    }
}