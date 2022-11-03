using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class WhileImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, var condition, var body))
        {
            TypeDeducer.ExpectType(condition, scope, context, modulename.Value, context.Environment.Boolean);

            var while_start = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_start"));
            AppendBlock(body, while_start, context, method, modulename, scope.CreateChildScope());

            var while_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_condition"));

            ConditionalJumpKind kind = ConditionalJumpKind.True;
            if (condition.Calls(CodeSymbols.Eq))
            {
                kind = ConditionalJumpKind.Equals;
            }
            else if (condition.Calls(CodeSymbols.NotEq))
            {
                kind = ConditionalJumpKind.NotEquals;
            }

            var while_end = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_end"));
            if (!condition.Calls(CodeSymbols.Bool))
            {
                AppendExpression(while_condition, condition[0], context.Environment.Boolean, context, scope, modulename);
                AppendExpression(while_condition, condition[1], context.Environment.Boolean, context, scope, modulename);
            }

            while_condition.Flow = new JumpConditionalFlow(while_start, kind);

            block.Flow = new JumpFlow(while_condition);

            while_start.Flow = new NothingFlow();
            while_end.Flow = new NothingFlow();

            return block.Graph.AddBasicBlock();
        }

        return null;
    }
}