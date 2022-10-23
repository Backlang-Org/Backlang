using Backlang.Contracts.Scoping.Items;
using Backlang.Contracts.TypeSystem;

namespace Backlang.Driver.Core.Implementors.Statements;

public class VariableImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        var decl = node.Args[1];

        var typename = ConversionUtils.GetQualifiedName(node.Args[0]);

        var elementType = TypeInheritanceStage.ResolveTypeWithModule(node.Args[0], context, modulename.Value, typename);

        var deducedValueType = TypeDeducer.Deduce(decl.Args[1], scope, context, modulename.Value);

        if (elementType == null)
        {
            elementType = deducedValueType;

            if (elementType == context.Environment.Void && !scope.TryGet<TypeScopeItem>(node.Args[0].Name.Name, out var _))
            {
                if (node.Args[0] is (_, (_, var tp))) //ToDo: Implement Helper function To Get Typename
                {
                    context.AddError(node, $"{tp.Name} cannot be resolved");
                }
            }
        }
        else
        {
            if (deducedValueType != null && !elementType.IsAssignableTo(deducedValueType) && deducedValueType != context.Environment.Void)
            {
                if (elementType is UnitType ut)
                {
                    if (ut != deducedValueType)
                    {
                        context.AddError(node, $"Unit Type mismatch {elementType} {deducedValueType}");
                    }
                    return block;
                }

                context.AddError(node, $"Type mismatch {elementType} {deducedValueType}");
            }
        }

        var varname = decl.Args[0].Name.Name;
        var isMutable = node.Attrs.Contains(LNode.Id(Symbols.Mutable));

        if (scope.Add(new VariableScopeItem
        {
            Name = varname,
            IsMutable = isMutable,
            Parameter = new Parameter(elementType, varname)
        }))
        {
            block.AppendParameter(new BlockParameter(elementType, varname, !isMutable));
        }
        else
        {
            context.AddError(decl.Args[0], $"{varname} already declared");
        }

        if (deducedValueType == null) return block;

        ImplementationStage.AppendExpression(block, decl.Args[1], elementType, context, scope, modulename);

        block.AppendInstruction(Instruction.CreateAlloca(elementType));

        return block;
    }
}