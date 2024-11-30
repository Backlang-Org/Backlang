using Backlang.Contracts.Scoping.Items;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class CallExpressionEmitter : IExpressionImplementor
{
    public bool CanHandle(LNode node)
    {
        return node.IsCall && !node.Calls(CodeSymbols.Tuple);
    }

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        if (scope.TryGet<FunctionScopeItem>(node.Name.Name, out var fn))
        {
            return ImplementationStage.AppendCall(context, block, node, fn.Overloads, scope, modulename,
                methodName: node.Name.Name);
        }

        if (TryGetFreeFunctionsFromNamespace(context, node.Range.Source.FileName, out var functions))
        {
            return ImplementationStage.AppendCall(context, block, node, functions, scope, modulename,
                methodName: node.Name.Name);
        }

        context.AddError(node, $"function {node.Name.Name} not found");
        return null;
    }

    private static bool TryGetFreeFunctionsFromNamespace(CompilerContext context, string filename,
        out IEnumerable<IMethod> functions)
    {
        var allFreeFunctions = new List<IMethod>();
        if (context.FileScope.ImportetNamespaces.TryGetValue(filename, out var importedNamespaces))
        {
            foreach (var ns in importedNamespaces.ImportedNamespaces)
            {
                if (context.Binder.TryResolveNamespace(ns, out var resolvedNs))
                {
                    if (!resolvedNs.Types.Any(_ => _.Name.ToString() == Names.FreeFunctions))
                    {
                        continue;
                    }

                    allFreeFunctions.AddRange(resolvedNs.Types.First(_ => _.Name.ToString() == Names.FreeFunctions)
                        .Methods);
                }
            }

            functions = allFreeFunctions;
            return true;
        }

        functions = null;
        return false;
    }
}