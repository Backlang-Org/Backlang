using Backlang.Contracts.Scoping.Items;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class AssignmentImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        if (parameters.node is (_, var left, var right))
        {
            var lt = TypeDeducer.Deduce(left, parameters.scope, parameters.context, parameters.modulename.Value);
            var rt = TypeDeducer.Deduce(right, parameters.scope, parameters.context, parameters.modulename.Value);
            TypeDeducer.ExpectType(right, parameters.scope, parameters.context, parameters.modulename.Value, lt);

            if (parameters.scope.TryGet<VariableScopeItem>(left.Name.Name, out var va))
            {
                if (!va.IsMutable)
                {
                    parameters.context.AddError(parameters.node, $"Cannot assing immutable variable '{va.Name}'");
                }

                var value = AppendExpression(parameters.block, right, rt, parameters.context,
                    parameters.scope, parameters.modulename.Value);
                var pointer = parameters.block.AppendInstruction(Instruction.CreateLoadLocal(va.Parameter));

                parameters.block.AppendInstruction(Instruction.CreateStore(lt, pointer, value));
            }
            else if (left is ("'.", var t, var c))
            {
                if (parameters.scope.TryGet<VariableScopeItem>(t.Name.Name, out var vsi))
                {
                    var field = vsi.Type.Fields.FirstOrDefault(_ => _.Name.ToString() == c.Name.Name);

                    if (field != null)
                    {
                        parameters.block.AppendInstruction(Instruction.CreateLoadLocalAdress(vsi.Parameter));
                        var value = AppendExpression(parameters.block, right, field.FieldType,
                            parameters.context, parameters.scope, parameters.modulename.Value);

                        var pointer = parameters.block.AppendInstruction(Instruction.CreateLoadField(field));

                        parameters.block.AppendInstruction(Instruction.CreateStore(field.FieldType, pointer, value));
                    }
                }
            }
            else
            {
                parameters.context.AddError(parameters.node, $"Variable '{left.Name.Name}' not found");
            }
        }

        return parameters.block;
    }
}