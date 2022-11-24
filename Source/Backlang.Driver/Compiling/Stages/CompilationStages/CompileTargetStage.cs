using Backlang.Codeanalysis.Core;
using Backlang.Core.CompilerService;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public sealed class CompileTargetStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Trees = null;

        var description = GetDescription(context);

        if (context.Options.Version != null)
            context.Assembly.AddAttribute(new VersionAttribute() { Version = Version.Parse(context.Options.Version) });

        context.CompilationTarget.BeforeCompiling(context);

        var assembly = context.CompilationTarget.Compile(description);

        if (!context.Playground.IsPlayground)
        {
            var resultPath = Path.Combine(context.TempOutputPath,
                            context.Options.OutputFilename);

            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            context.OutputStream = File.OpenWrite(resultPath);
        }

        assembly.WriteTo(context.OutputStream);

        context.CompilationTarget.AfterCompiling(context);

        return await next.Invoke(context);
    }

    private static AssemblyContentDescription GetDescription(CompilerContext context)
    {
        var attributes = new AttributeMap();

        if (context.Options.OutputType == "MacroLib")
        {
            attributes = new AttributeMap(new DescribedAttribute(Utils.ResolveType(context.Binder, typeof(MacroLibAttribute))));
        }

        var entryPoint = GetEntryPoint(context);

        context.Assembly.IsLibrary = entryPoint == null && context.Options.OutputType == "Library";

        return new(context.Assembly.Name.Qualify(),
            attributes, context.Assembly, entryPoint, context.Environment);
    }

    private static IMethod GetEntryPoint(CompilerContext context)
    {
        if (string.IsNullOrEmpty(context.Options.OutputType))
        {
            context.Options.OutputType = "Exe";
        }

        if (context.Options.OutputType != "Exe")
        {
            return null;
        }

        var entryPoint = context.Assembly.Types.SelectMany(_ => _.Methods)
            .FirstOrDefault(_ => _.Name.ToString() == Names.MainMethod);

        if (entryPoint == null)
        {
            context.Messages.Add(Message.Error(ErrorID.RunnableTypeButNoEntrypoint));
        }

        return entryPoint;
    }
}