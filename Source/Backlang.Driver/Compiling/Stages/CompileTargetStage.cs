using Backlang.Codeanalysis.Parsing;
using Backlang.Core.CompilerService;
using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;

namespace Backlang.Driver.Compiling.Stages;

public sealed class CompileTargetStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Trees = null;

        if (!context.Messages.Any())
        {
            AssemblyContentDescription description = GetDescription(context);

            var assembly = context.CompilationTarget.Compile(description);
            assembly.WriteTo(File.OpenWrite(Path.Combine(context.TempOutputPath,
                context.OutputFilename)));

            var runtimeConfigStream = typeof(CompileTargetStage).Assembly.GetManifestResourceStream("Backlang.Driver.compilation.runtimeconfig.json");
            var jsonStream = File.OpenWrite($"{Path.Combine(context.TempOutputPath, Path.GetFileNameWithoutExtension(context.OutputFilename))}.runtimeconfig.json");

            runtimeConfigStream.CopyTo(jsonStream);

            jsonStream.Close();
            runtimeConfigStream.Close();
        }

        return await next.Invoke(context);
    }

    private static AssemblyContentDescription GetDescription(CompilerContext context)
    {
        var attributes = new AttributeMap();

        if (context.OutputType == "MacroLib")
        {
            attributes = new AttributeMap(new DescribedAttribute(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(MacroLibAttribute))));
        }

        return new(context.Assembly.Name.Qualify(),
            attributes, context.Assembly, GetEntryPoint(context), context.Environment);
    }

    private static IMethod GetEntryPoint(CompilerContext context)
    {
        if (context.OutputType != "Exe")
        {
            return null;
        }

        var entryPoint = context.Assembly.Types
            .FirstOrDefault(_ => _.Name.ToString() == Names.ProgramClass)
            .Methods.FirstOrDefault(_ => _.Name.ToString() == Names.MainMethod && _.IsStatic);

        if (entryPoint == null)
        {
            context.Messages.Add(Message.Error("Got OutputType 'Exe' but couldn't find entry point."));
        }

        return entryPoint;
    }
}