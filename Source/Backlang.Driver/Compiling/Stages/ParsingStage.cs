using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ParsingStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        var hasError = false;
        foreach (var filename in context.InputFiles)
        {
            if (File.Exists(filename))
            {
                var tree = CompilationUnit.FromFile(filename);

                context.Trees.Add(tree);

                context.Messages.AddRange(tree.Messages);
            }
            else
            {
                hasError = true;
                context.Messages.Add(Message.Error(null, $"File '{filename}' does not exists", 0, 0));
            }
        }

        if (!hasError)
        {
            return await next.Invoke(context);
        }

        return context;
    }
}