using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class IfImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, (_, var condition, var body, var el)))
        {
            TypeDeducer.ExpectType(condition, scope, context, modulename.Value, context.Environment.Boolean);

            var ifBlock = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if"));
            var after = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("after"));

            after.Flow = new NothingFlow();

            var kind = ConditionalJumpKind.True;

            if (!condition.Calls(CodeSymbols.Bool))
            {
                AppendExpression(block, condition, context.Environment.Boolean, context, scope, modulename);

                block.Flow = new JumpConditionalFlow(after, kind);
            }
            ifBlock.Flow = new JumpConditionalFlow(after, kind);
            AppendBlock(body, ifBlock, context, method, modulename, scope.CreateChildScope());

            //Todo: fix else
            if (el != LNode.Missing)
            {
                var elseBlock = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("else"));
                AppendBlock(el, elseBlock, context, method, modulename, scope.CreateChildScope());
                block.Flow = new JumpConditionalFlow(after, kind);
            }

            return after;
        }

        return null;
    }
}