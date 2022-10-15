using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ReportErrorStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var msg in context.Messages)
        {
            if (msg.Severity == MessageSeverity.Warning)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine($"[{msg.Severity}]: {msg}");
            Console.ResetColor();
        }

        if (context.Messages.Any(_ => _.Severity == MessageSeverity.Error))
        {
            Environment.Exit(1337);
        }

        return await next.Invoke(context);
    }
}