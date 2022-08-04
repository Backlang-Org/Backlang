using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class WhileImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, var condition, var body))
        {
            TypeDeducer.ExpectType(condition, scope, context, context.Environment.Boolean);

            var while_start = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_start"));
            AppendBlock(body, while_start, context, method, modulename, scope.CreateChildScope());

            var while_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_condition"));
            AppendExpression(while_condition, condition, context.Environment.Boolean, method, context, scope);
            while_condition.Flow = new JumpFlow(while_start);

            var while_end = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_end"));
            block.Flow = new JumpFlow(while_condition);

            while_start.Flow = new JumpFlow(while_end);

            if (condition.Calls(CodeSymbols.Bool))
            {
                while_end.Flow = new JumpConditionalFlow(while_start, ConditionalJumpKind.True);
            }
            else
            {
                AppendExpression(block, condition, method.ParentType, method, context, scope);
                while_end.Flow = new JumpConditionalFlow(while_start, ConditionalJumpKind.Equals);
            }

            return block.Graph.AddBasicBlock();
        }

        return null;
    }
}