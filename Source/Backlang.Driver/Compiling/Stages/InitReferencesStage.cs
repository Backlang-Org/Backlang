using Backlang.Contracts;
using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class InitReferencesStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.CompilationTarget.InitReferences(context);

        return await next.Invoke(context);
    }
}