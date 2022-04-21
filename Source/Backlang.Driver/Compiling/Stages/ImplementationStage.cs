using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            CollectImplementations(context, tree);
        }

        return await next.Invoke(context);
    }

    private void CollectImplementations(CompilerContext context, CompilationUnit tree)
    {
        var implementations = tree.Body.Where(_ => _.IsCall && _.Name == Symbols.Implementation);

        foreach (var st in implementations)
        {
            var targetType = (DescribedType)IntermediateStage.GetType(st.Args[0], context.Binder);
            var body = st.Args[1].Args;

            foreach (var node in body)
            {
                if (node.Name == CodeSymbols.Fn)
                {
                    var fn = IntermediateStage.ConvertFunction(context, targetType, node);

                    targetType.AddMethod(fn);
                }
            }
        }
    }
}