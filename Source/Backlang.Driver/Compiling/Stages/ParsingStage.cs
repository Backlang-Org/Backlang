using Backlang.Codeanalysis.Parsing.AST;
using Flo;

namespace Backlang_Compiler.Compiling.Stages;

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
            }
            else
            {
                hasError = true;
                Console.WriteLine($"File '{filename}' does not exist.");
            }
        }

        if (!hasError)
        {
            return await next.Invoke(context);
        }

        return context;
    }
}