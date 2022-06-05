using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Pipeline;

namespace Backlang.Driver.Compiling.Stages;

public sealed class CompileTargetStage : IHandler<CompilerContext, CompilerContext>
{
    private Dictionary<string, ITarget> _targets = new();

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        if (!context.Messages.Any())
        {
            if (_targets.ContainsKey(context.Target))
            {
                AssemblyContentDescription description = GetDescription(context);

                var assembly = _targets[context.Target].Compile(description);
                assembly.WriteTo(File.OpenWrite(Path.Combine(context.TempOutputPath, context.OutputFilename + ".dll")));
            }
            else
            {
                Console.Error.WriteLine($"Target '{context.Target}' not found");
                Environment.Exit(1);
            }
        }

        return await next.Invoke(context);
    }

    private AssemblyContentDescription GetDescription(CompilerContext context)
    {
        var name = new SimpleName("Test").Qualify();
        var attributes = new AttributeMap();

        return new(name, attributes, context.Assembly, context.EntryPoint);
    }
}