using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class WhileImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        if (parameters.node is (_, var condition, var body))
        {
            TypeDeducer.ExpectType(condition, parameters.scope, parameters.context, parameters.modulename.Value,
                parameters.context.Environment.Boolean);

            var while_start = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_start"));
            var while_condition = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_condition"));
            var while_end = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_end"));
            var while_after = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_after"));
            while_after.Flow = new NothingFlow();

            AppendBlock(body, while_start, parameters.context, parameters.method, parameters.modulename,
                parameters.scope.CreateChildScope());

            if (!condition.Calls(CodeSymbols.Bool) && condition.Name.ToString().StartsWith("'") && condition.ArgCount == 2)
            {
                AppendExpression(while_condition, condition, parameters.context.Environment.Boolean,
                    parameters.context, parameters.scope, parameters.modulename);
            }
            else
            {
                AppendExpression(while_condition, condition, parameters.context.Environment.Boolean,
                    parameters.context, parameters.scope, parameters.modulename);
            }

            while_condition.Flow = new JumpConditionalFlow(while_start, ConditionalJumpKind.True);

            parameters.block.Flow = new JumpFlow(while_condition);

            while_end.Flow = new NothingFlow();

            return while_after;
        }

        return null;
    }
}