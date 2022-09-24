using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ParsingStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        Parallel.ForEachAsync(context.InputFiles, (filename, ct) => {
            if (File.Exists(filename))
            {
                var tree = CompilationUnit.FromFile(filename);

                context.Trees.Add(tree);

                context.Messages.AddRange(tree.Messages);
            }
            else
            {
                context.Messages.Add(Message.Error($"File '{filename}' does not exists", SourceRange.Synthetic));
            }

            return ValueTask.CompletedTask;
        }).Wait();

        return await next.Invoke(context);
    }
}