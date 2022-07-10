using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Flo;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ExpandImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    private static List<Symbol> _primitiveTypes = new()
    {
        (Symbol)"u8",
        (Symbol)"u16",
        (Symbol)"u32",
        (Symbol)"u64",

        (Symbol)"i8",
        (Symbol)"i16",
        (Symbol)"i32",
        (Symbol)"i64",

        (Symbol)"f16",
        (Symbol)"f32",
        (Symbol)"f64",
    };

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            ExpandImplementations(context, tree);
        }

        return await next.Invoke(context);
    }

    private static LNode GenerateRangeTargets(LNode targets)
    {
        var min = targets.Args[0].Name;
        var max = targets.Args[1].Name;

        var minIndex = _primitiveTypes.IndexOf(min);
        var maxIndex = _primitiveTypes.IndexOf(max);
        var difference = maxIndex - minIndex;

        return LNode.Call(Symbols.ToExpand, LNode.List(Enumerable.Range(minIndex, difference + 1).Select(i => SyntaxTree.Type(_primitiveTypes[i].Name, LNode.List())).ToArray()));
    }

    private void ExpandImplementations(CompilerContext context, CompilationUnit tree)
    {
        var newBody = new LNodeList();

        for (var i = 0; i < tree.Body.Count; i++)
        {
            var node = tree.Body[i];

            if (node.IsCall && node.Name == Symbols.Implementation)
            {
                //node = node.Args[0];

                var targets = GetTargets(node.Args[0].Args[0]);

                var body = node.Args[0].Args[1].Args;

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

    private LNode GetTargets(LNode targets)
    {
        if (targets.Calls(Symbols.Range))
        {
            return GenerateRangeTargets(targets);
        }
        else if (targets.Calls(Symbols.ToExpand))
        {
            var newTargets = new LNodeList();
            foreach (var arg in targets.Args)
            {
                if (arg.Calls(Symbols.Range))
                {
                    var rangeTargets = GenerateRangeTargets(arg).Args;
                    newTargets.AddRange(rangeTargets);
                }
                else
                {
                    newTargets.Add(arg);
                }
            }

            return targets.WithArgs(newTargets);
        }

        return targets;
    }
}