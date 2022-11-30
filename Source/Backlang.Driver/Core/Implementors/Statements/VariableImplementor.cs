using Backlang.Codeanalysis.Core;
using Backlang.Contracts.Scoping.Items;
using Backlang.Contracts.TypeSystem;

namespace Backlang.Driver.Core.Implementors.Statements;

public class VariableImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        var decl = parameters.node.Args[1];

        var typename = ConversionUtils.GetQualifiedName(parameters.node.Args[0]);

        var elementType = TypeInheritanceStage.ResolveTypeWithModule(parameters.node.Args[0],
            parameters.context, parameters.modulename.Value, typename);

        var deducedValueType = TypeDeducer.Deduce(decl.Args[1], parameters.scope,
            parameters.context, parameters.modulename.Value);

        if (elementType == null)
        {
            elementType = deducedValueType;

            if (elementType == parameters.context.Environment.Void && !parameters.scope.TryGet<TypeScopeItem>(parameters.node.Args[0].Name.Name, out var _))
            {
                if (parameters.node.Args[0] is (_, (_, var tp))) //ToDo: Implement Helper function To Get Typename
                {
                    parameters.context.AddError(parameters.node,
                        new(ErrorID.CannotBeResolved, tp.Name.ToString()));
                }
            }
        }
        else
        {
            if (deducedValueType != null && !elementType.IsAssignableTo(deducedValueType) &&
                deducedValueType != parameters.context.Environment.Void)
            {
                if (elementType is UnitType ut)
                {
                    if (ut != deducedValueType)
                    {
                        parameters.context.AddError(parameters.node,
                            new(ErrorID.UnitTypeMismatch, elementType.ToString(), deducedValueType.ToString()));
                    }
                    return parameters.block;
                }

                parameters.context.AddError(parameters.node,
                    new(ErrorID.TypeMismatch, elementType.ToString(), deducedValueType.ToString()));
            }
        }

        var varname = decl.Args[0].Name.Name;
        var isMutable = parameters.node.Attrs.Contains(LNode.Id(Symbols.Mutable));

        if (parameters.scope.Add(new VariableScopeItem
        {
            Name = varname,
            IsMutable = isMutable,
            Parameter = new Parameter(elementType, varname)
        }))
        {
            parameters.block.AppendParameter(new BlockParameter(elementType, varname, !isMutable));
        }
        else
        {
            parameters.context.AddError(decl.Args[0], new(ErrorID.AlreadyDeclared, varname));
        }

        if (deducedValueType == null) return parameters.block;

        ImplementationStage.AppendExpression(parameters.block, decl.Args[1], elementType, parameters.context,
            parameters.scope, parameters.modulename);

        parameters.block.AppendInstruction(Instruction.CreateAlloca(elementType));

        return parameters.block;
    }
}