using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class IfImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, (_, var condition, var body, var el)))
        {
            TypeDeducer.ExpectType(condition, scope, context, context.Environment.Boolean);

            var ifBlock = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if"));
            AppendBlock(body, ifBlock, context, method, modulename, scope.CreateChildScope());

            if (el != LNode.Missing)
            {
                var elseBlock = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("else"));
                AppendBlock(el, elseBlock, context, method, modulename, scope.CreateChildScope());
            }

            var if_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if_condition"));
            if (condition.Calls(CodeSymbols.Bool))
            {
                if_condition.Flow = new JumpConditionalFlow(ifBlock, ConditionalJumpKind.True);
            }
            else
            {
                AppendExpression(if_condition, condition, context.Environment.Boolean, method);
                if_condition.Flow = new JumpConditionalFlow(ifBlock, ConditionalJumpKind.Equals);
            }

            block.Flow = new JumpFlow(if_condition);

            var after = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("after"));
            ifBlock.Flow = new JumpFlow(after);

            return after;
        }

        return null;
    }
}