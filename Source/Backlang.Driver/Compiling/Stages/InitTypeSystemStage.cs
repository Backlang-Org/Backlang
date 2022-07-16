using Backlang.Driver.Compiling.Targets.bs2k;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Stages;

public sealed class InitTypeSystemStage : IHandler<CompilerContext, CompilerContext>
{
    private readonly Dictionary<string, ICompilationTarget> _targets = new();

    public InitTypeSystemStage()
    {
        AddTarget<DotNetTarget>();
        AddTarget<BS2KTarget>();
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Binder = new TypeResolver();

        if (string.IsNullOrEmpty(context.Target))
        {
            context.Target = "dotnet";
            context.OutputFilename += ".dll";
        }

        if (context.OutputType == "dotnet")
        {
            context.OutputType = "Exe";
        }

        if (_targets.ContainsKey(context.Target))
        {
            var compilationTarget = _targets[context.Target];

            context.CompilationTarget = compilationTarget;
            context.Environment = compilationTarget.Init(context.Binder);
        }

        var consoleType = context.Binder.ResolveTypes(new SimpleName("Console").Qualify("System")).FirstOrDefault();

        context.writeMethods = consoleType?.Methods.Where(
            method => method.Name.ToString() == "Write"
                && method.IsStatic
                && method.ReturnParameter.Type == context.Environment.Void);

        return await next.Invoke(context);
    }

    private void AddTarget<T>()
                    where T : ICompilationTarget, new()
    {
        var target = new T();
        _targets.Add(target.Name, target);
    }
}