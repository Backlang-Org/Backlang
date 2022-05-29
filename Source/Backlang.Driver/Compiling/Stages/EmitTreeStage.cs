using Flo;
using System.Text;

namespace Backlang.Driver.Compiling.Stages;

public sealed class EmitTreeStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        if (!context.Messages.Any())
        {
            var sb = new StringBuilder();
            var tree = context.Trees.FirstOrDefault();

            if (tree == null)
            {
                return context;
            }

            foreach (var node in tree.Body)
            {
                sb.AppendLine(node.ToString());
            }

            File.WriteAllText(Path.Combine(context.TempOutputPath, context.OutputFilename + ".txt"), sb.ToString());
        }

        return await next.Invoke(context);
    }
}