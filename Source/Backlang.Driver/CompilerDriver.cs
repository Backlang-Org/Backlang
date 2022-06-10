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

           cfg.When(_ => !_.Messages.Any(), _ => {
               _.Add<InitTypeSystemStage>();
           });

           cfg.When(_ => _.References.Any(), _ => {
               _.Add<InitReferencesStage>();
           });

           cfg.When(_ => !_.Messages.Any(), _ => {
               _.Add<ExpandMacrosStage>();
           });

           cfg.When(_ => !_.Messages.Any(), _ => {
               _.Add<IntermediateStage>();
           });

           cfg.When(_ => !_.Messages.Any(), _ => {
               _.Add<TypeInheritanceStage>();
           });

           cfg.When(_ => !_.Messages.Any(), _ => {
               _.Add<ExpandImplementationStage>();
           });

           cfg.When(_ => !_.Messages.Any(), _ => {
               _.Add<ImplementationStage>();
           });

           cfg.When(_ => !_.Messages.Any() && _.OutputTree, _ => {
               _.Add<EmitTreeStage>();
           });

           cfg.When(_ => !_.Messages.Any(), _ => {
               _.Add<CompileTargetStage>();
           });

           cfg.When(_ => _.Messages.Any(), _ => {
               _.Add<ReportErrorStage>();
           });
       }
       );

        await pipeline.Invoke(context);
    }
}