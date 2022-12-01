using Backlang.Codeanalysis.Core;
using Backlang.Contracts.Scoping.Items;
using Backlang.Contracts.TypeSystem;

namespace Backlang.Driver.Core.Implementors.Statements;

public class VariableImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        var decl = node.Args[1];

        var typename = ConversionUtils.GetQualifiedName(node.Args[0]);

        var elementType = TypeInheritanceStage.ResolveTypeWithModule(node.Args[0],
            context, modulename.Value, typename);

        var deducedValueType = TypeDeducer.Deduce(decl.Args[1], scope,
            context, modulename.Value);

        if (elementType == null)
        {
            elementType = deducedValueType;

            if (elementType == context.Environment.Void && !scope.TryGet<TypeScopeItem>(node.Args[0].Name.Name, out var _))
            {
                if (node.Args[0] is (_, (_, var tp))) //ToDo: Implement Helper function To Get Typename
                {
                    context.AddError(node,
                        new(ErrorID.CannotBeResolved, tp.Name.ToString()));
                }
            }
        }
        else
        {
            if (deducedValueType != null && !elementType.IsAssignableTo(deducedValueType) &&
                deducedValueType != context.Environment.Void)
            {
                if (elementType is UnitType ut)
                {
                    if (ut != deducedValueType)
                    {
                        context.AddError(node,
                            new(ErrorID.UnitTypeMismatch, elementType.ToString(), deducedValueType.ToString()));
                    }
                    return block;
                }

                context.AddError(node,
                    new(ErrorID.TypeMismatch, elementType.ToString(), deducedValueType.ToString()));
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
            context.AddError(decl.Args[0], new(ErrorID.AlreadyDeclared, varname));
        }

        if (deducedValueType == null) return block;

        ImplementationStage.AppendExpression(block, decl.Args[1], elementType, context,
            scope, modulename);

        block.AppendInstruction(Instruction.CreateAlloca(elementType));

        return block;
    }
}