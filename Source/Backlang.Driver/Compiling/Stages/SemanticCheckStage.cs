using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class SemanticCheckStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        Parallel.ForEachAsync(context.Trees, (tree, ct) => {
            SemanticChecker.Do(tree, context);

            return ValueTask.CompletedTask;
        }).Wait();

        return await next.Invoke(context);
    }
}