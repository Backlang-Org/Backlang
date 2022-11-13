using Flo;
using System.Diagnostics;

namespace Backlang.Driver.Compiling.Stages;

public sealed class WaitForDebuggerStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        while (!Debugger.IsAttached)
        {
        }

        return await next.Invoke(context);
    }
}