using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;
using System.Runtime.CompilerServices;

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
            var targetType = (DescribedType)IntermediateStage.GetType(st.Args[0], context);
            var body = st.Args[1].Args;

            foreach (var node in body)
            {
                if (node.Name == CodeSymbols.Fn)
                {
                    if (targetType.Parent.Assembly == context.Assembly)
                    {
                        var fn = IntermediateStage.ConvertFunction(context, targetType, node);
                        targetType.AddMethod(fn);
                    }
                    else
                    {
                        var fn = IntermediateStage.ConvertFunction(context, context.ExtensionsType, node);

                        var thisParameter = new Parameter(targetType, "this");
                        var param = (IList<Parameter>)fn.Parameters;

                        param.Insert(0, thisParameter);

                        var extType = ClrTypeEnvironmentBuilder
                            .ResolveType(context.Binder, typeof(ExtensionAttribute));

                        fn.AddAttribute(new DescribedAttribute(extType));

                        context.ExtensionsType.AddMethod(fn);
                    }
                }
            }
        }
    }
}