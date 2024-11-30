using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ParsingStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context,
        Func<CompilerContext, Task<CompilerContext>> next)
    {
        if (context.Playground.IsPlayground)
        {
            var tree = CompilationUnit.FromText(context.Playground.Source);

            ApplyTree(context, tree);

            return await next.Invoke(context);
        }

        ParseSourceFiles(context);

        return await next.Invoke(context);
    }

    private static void ParseSourceFiles(CompilerContext context)
    {
        Parallel.ForEachAsync(context.Options.InputFiles, (filename, ct) => {
            if (File.Exists(filename))
            {
                var tree = CompilationUnit.FromFile(filename);

                ApplyTree(context, tree);
            }
            else
            {
                context.Messages.Add(Message.Error($"File '{filename}' does not exists", SourceRange.Synthetic));
            }

            return ValueTask.CompletedTask;
        }).Wait();
    }

    private static void ApplyTree(CompilerContext context, CompilationUnit tree)
    {
        context.Trees.Add(tree);

        context.Messages.AddRange(tree.Messages);
    }
}