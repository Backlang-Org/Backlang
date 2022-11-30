using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class IfImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        if (parameters.node is (_, (_, var condition, var body, var el)))
        {
            TypeDeducer.ExpectType(condition, parameters.scope, parameters.context, parameters.modulename.Value,
                parameters.context.Environment.Boolean);

            var ifBlock = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if"));
            var after = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("after"));

            after.Flow = new NothingFlow();

            var kind = ConditionalJumpKind.True;

            if (!condition.Calls(CodeSymbols.Bool))
            {
                AppendExpression(parameters.block, condition, parameters.context.Environment.Boolean,
                    parameters.context, parameters.scope, parameters.modulename);

                parameters.block.Flow = new JumpConditionalFlow(after, kind);
            }
            ifBlock.Flow = new JumpConditionalFlow(after, kind);
            AppendBlock(body, ifBlock, parameters.context, parameters.method, parameters.modulename,
                parameters.scope.CreateChildScope());

            //Todo: fix else
            if (el != LNode.Missing)
            {
                var elseBlock = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("else"));
                AppendBlock(el, elseBlock, parameters.context, parameters.method, parameters.modulename,
                    parameters.scope.CreateChildScope());
                parameters.block.Flow = new JumpConditionalFlow(after, kind);
            }

            return after;
        }

        return null;
    }
}