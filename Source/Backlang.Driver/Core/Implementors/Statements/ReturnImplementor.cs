using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ReturnImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        if (node.ArgCount == 1)
        {
            var valueNode = node.Args[0];

            AppendExpression(block, valueNode,
                TypeDeducer.Deduce(valueNode, scope, context, modulename.Value),
                context, scope, modulename);

            block.Flow = new ReturnFlow();
        }
        else
        {
            block.Flow = new ReturnFlow();
        }

        return block;
    }
}