using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ReportErrorStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var msg in context.Messages)
        {
            Console.WriteLine(msg.ToString());
        }

        Environment.Exit(1337);

        return await next.Invoke(context);
    }
}