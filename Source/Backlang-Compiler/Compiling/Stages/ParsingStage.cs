using Flo;
using Backlang.Codeanalysis.Parsing.AST;

namespace Backlang_Compiler.Compiling.Stages;

public class ParsingStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        bool hasError = false;
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
            }
        }

        if (!hasError)
        {
            return await next.Invoke(context);
        }
        else
        {
            return context;
        }
    }
}
