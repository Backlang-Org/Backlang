using Backlang.Core.CompilerService;
using Backlang.Driver.Compiling.Targets;
using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;

namespace Backlang.Driver.Compiling.Stages;

public sealed class CompileTargetStage : IHandler<CompilerContext, CompilerContext>
{
    private Dictionary<string, ITarget> _targets = new();

    public CompileTargetStage()
    {
        AddTarget<DotNetTarget>();
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        if (!context.Messages.Any())
        {
            if (string.IsNullOrEmpty(context.Target))
            {
                context.Target = "dotnet";
                context.OutputFilename += ".dll";
            }

            if (_targets.ContainsKey(context.Target))
            {
                AssemblyContentDescription description = GetDescription(context);

                var assembly = _targets[context.Target].Compile(description);
                assembly.WriteTo(File.OpenWrite(Path.Combine(context.TempOutputPath, context.OutputFilename)));

                var runtimeConfigStream = typeof(CompileTargetStage).Assembly.GetManifestResourceStream("Backlang.Driver.compilation.runtimeconfig.json");
                var jsonStream = File.OpenWrite($"{context.OutputFilename}.runtimeconfig.json");

                runtimeConfigStream.CopyTo(jsonStream);

                jsonStream.Close();
                runtimeConfigStream.Close();
            }
            else
            {
                Console.Error.WriteLine($"Target '{context.Target}' not found");
                Environment.Exit(1);
            }
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
        return context.Assembly.Types
            .First(_ => _.Name.ToString() == Names.ProgramClass)
            .Methods.First(_ => _.Name.ToString() == Names.MainMethod && _.IsStatic);
    }

    private void AddTarget<T>()
                    where T : ITarget, new()
    {
        var target = new T();
        _targets.Add(target.Name, target);
    }
}