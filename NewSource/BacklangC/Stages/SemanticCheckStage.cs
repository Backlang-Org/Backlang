using BacklangC.Core.Semantic;
using Flo;

namespace BacklangC.Stages;

public sealed class SemanticCheckStage : IHandler<Driver, Driver>
{
    public async Task<Driver> HandleAsync(Driver context, Func<Driver, Task<Driver>> next)
    {
        Parallel.ForEachAsync(context.Trees, (tree, ct) => {
            SemanticChecker.Do(tree, context);

            return ValueTask.CompletedTask;
        }).Wait();

        return await next.Invoke(context);
    }
}