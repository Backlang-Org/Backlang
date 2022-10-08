using Backlang.Contracts.Scoping.Items;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class AssignmentImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, var left, var right))
        {
            var lt = TypeDeducer.Deduce(left, scope, context, modulename.Value);
            var rt = TypeDeducer.Deduce(right, scope, context, modulename.Value);
            TypeDeducer.ExpectType(right, scope, context, modulename.Value, lt);

            if (scope.TryGet<VariableScopeItem>(left.Name.Name, out var va))
            {
                if (!va.IsMutable)
                {
                    context.AddError(node, $"Cannot assing immutable variable '{va.Name}'");
                }

                var value = AppendExpression(block, right, rt, context, scope, modulename.Value);

                block.AppendInstruction(Instruction.CreateStore(lt, new ValueTag(va.Parameter.Name.ToString()), value));
            }
            else if (left is ("'.", var t, var c))
            {
                if (scope.TryGet<VariableScopeItem>(t.Name.Name, out var vsi))
                {
                    var field = vsi.Type.Fields.FirstOrDefault(_ => _.Name.ToString() == c.Name.Name);

                    if (field != null)
                    {
                        var pointer = block.AppendInstruction(Instruction.CreateLoadField(field));
                        var value = AppendExpression(block, right, field.FieldType, context, scope, modulename.Value);

                        block.AppendInstruction(Instruction.CreateStore(field.FieldType, pointer, value));
                    }
                }
            }
            else
            {
                context.AddError(node, $"Variable '{left.Name.Name}' not found");
            }
        }

        return block;
    }
}