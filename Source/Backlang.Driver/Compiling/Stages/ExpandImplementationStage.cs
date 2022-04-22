using Backlang.Codeanalysis.Parsing.AST;
using Flo;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ExpandImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            ExpandImplemtations(context, tree);
        }

        return await next.Invoke(context);
    }

    private void ExpandImplemtations(CompilerContext context, CompilationUnit tree)
    {
        var newBody = new LNodeList();

        foreach (var node in tree.Body)
        {
            if (node.IsCall && node.Name == Symbols.Implementation)
            {
                var targets = node.Args[0];
                var body = node.Args[1].Args;

                if (targets.Name != Symbols.ToExpand)
                {
                    newBody.Add(node);

                    continue;
                }

                foreach (var target in targets.Args)
                {
                    var impl = node.Clone();
                    impl = impl.WithArgChanged(0, target);

                    newBody.Add(impl);
                }
            }
            else
            {
                newBody.Add(node);
            }
        }

        tree.Body = newBody;
    }
}