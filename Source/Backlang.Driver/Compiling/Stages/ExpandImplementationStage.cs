using Backlang.Codeanalysis.Parsing.AST;
using Flo;
using Loyc;
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

                    impl = impl.WithArgs(impl.RecursiveReplace((node) => {
                        var body = node.Args[1];

                        if (body.Name != CodeSymbols.Fn)
                        {
                            var args = body.Args[0];

                            var retType = args.Args[0].Args[0];

                            if (retType.Name == (Symbol)"SELF")
                            {
                                var newFn = args.WithArgChanged(0, target);
                                body = body.WithArgChanged(0, newFn);
                                node = node.WithArgChanged(1, body);
                            }
                        }

                        return node.Args;
                    }));

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