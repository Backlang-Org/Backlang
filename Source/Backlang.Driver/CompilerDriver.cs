using Backlang.Codeanalysis.Parsing;
using Backlang.Driver.Compiling.Stages;

namespace Backlang.Driver;

public class CompilerDriver
{
    public static async void Compile(CompilerContext context)
    {
        if (string.IsNullOrEmpty(context.TempOutputPath))
            context.TempOutputPath = Environment.CurrentDirectory;

        var hasError = (List<Message> messages) => messages.Any(_ => _.Severity == MessageSeverity.Error);

        var pipeline = Flo.Pipeline.Build<CompilerContext, CompilerContext>(
       cfg => {
           cfg.Add<ParsingStage>();

           cfg.When(_ => !hasError(_.Messages) && _.OutputTree, _ => {
               _.Add<EmitTreeStage>();
           });

           cfg.When(_ => !hasError(_.Messages), _ => {
               _.Add<InitTypeSystemStage>();
           });

           cfg.When(_ => _.References.Any(), _ => {
               _.Add<InitReferencesStage>();
           });

           cfg.When(_ => !hasError(_.Messages), _ => {
               _.Add<ExpandMacrosStage>();
           });

           cfg.When(_ => !hasError(_.Messages), _ => {
               _.Add<IntermediateStage>();
           });

           cfg.When(_ => !hasError(_.Messages), _ => {
               _.Add<TypeInheritanceStage>();
           });

           cfg.When(_ => !hasError(_.Messages), _ => {
               _.Add<ExpandImplementationStage>();
           });

           cfg.When(_ => !hasError(_.Messages), _ => {
               _.Add<ImplementationStage>();
           });

           cfg.When(_ => !hasError(_.Messages), _ => {
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