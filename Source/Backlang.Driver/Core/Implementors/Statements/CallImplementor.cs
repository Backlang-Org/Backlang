using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class CallImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is ("'.", var target, var callee) && target is ("this", _))
        {
            //AppendThis(block, method.ParentType); // we do that already in AppendCall

            var type = method.ParentType;
            var call = type.Methods.FirstOrDefault(_ => _.Name.ToString() == callee.Name.Name);

            if (callee != null)
            {
                AppendCall(context, block, callee, type.Methods, scope);
            }
            else
            {
                context.AddError(node, $"Cannot find function '{callee.Name.Name}'");
            }
        }
        else
        {
            // ToDo: other things and so on...
        }

        return block;
    }
}