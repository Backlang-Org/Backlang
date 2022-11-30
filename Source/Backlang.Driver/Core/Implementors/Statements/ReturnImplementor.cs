using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ReturnImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        if (parameters.node.ArgCount == 1)
        {
            var valueNode = parameters.node.Args[0];

            AppendExpression(parameters.block, valueNode,
                TypeDeducer.Deduce(valueNode, parameters.scope, parameters.context, parameters.modulename.Value),
                parameters.context, parameters.scope, parameters.modulename);

            parameters.block.Flow = new ReturnFlow();
        }
        else
        {
            parameters.block.Flow = new ReturnFlow();
        }

        return parameters.block;
    }
}