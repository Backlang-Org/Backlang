using Backlang.Driver;
using Backlang.Driver.Compiling.Typesystem;
using Flo;
using System.Reflection;

namespace Backlang.Driver.Compiling.Stages;

public sealed class InitReferencesStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var r in context.References)
        {
            var assembly = Assembly.LoadFrom(r);

            var refLib = ClrTypeEnvironmentBuilder.CollectTypes(assembly);

            context.Binder.AddAssembly(refLib);

            ClrTypeEnvironmentBuilder.FillTypes(assembly, context.Binder);
        }

        return await next.Invoke(context);
    }
}