using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class ArrayExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.Array);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block, IType elementType,
        CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var value = block.AppendInstruction(Instruction.CreateConstant(new IntegerConstant(node.ArgCount), context.Environment.Int32));
        var counter = block.AppendInstruction(Instruction.CreateLoad(context.Environment.Int32, value));

        if (elementType.FullName.Qualifier is GenericName gn)
        {
            elementType = context.Binder.ResolveTypes(gn.TypeArgumentNames[0]).FirstOrDefault();
        }

        var arrayValuesType = GetOrAddArrayValueType(context.Environment.MakeArrayType(elementType, 1), context, out DescribedField field); //Todo: replace rank

        field.InitialValue = new[] { 1,2,3 };

        //Todo: only emit this if values are primitive values otherwise emit storeelementref
        var args = new List<ValueTag> {
            block.AppendInstruction(Instruction.CreateAllocaArray(elementType, counter)),
            block.AppendInstruction(Instruction.CreateCopy(arrayValuesType, null))
        };

        var initArrayMethod = context.Binder.FindFunction("System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array, System.RuntimeFieldHandle)");

        return block.AppendInstruction(Instruction.CreateCall(initArrayMethod, MethodLookup.Static, args));
    }

    private static DescribedType GetOrAddArrayValueType(IType elementType, CompilerContext context, out IField field)
    {
        var arrayValuesType = (DescribedType)context.Binder.ResolveTypes(new SimpleName(Names.ArrayValues).Qualify("")).FirstOrDefault();

        if (arrayValuesType == null)
        {
            arrayValuesType = new DescribedType(new SimpleName(Names.ArrayValues).Qualify(""), context.Assembly)
            {
                IsStatic = true
            };

            Utils.AddCompilerGeneratedAttribute(context.Binder, arrayValuesType);

            context.Assembly.AddType(arrayValuesType);
        }

        var randomFieldName = Utils.GenerateIdentifier();
        field = new DescribedField(arrayValuesType, new SimpleName(randomFieldName), true, elementType);

        arrayValuesType.AddField(field);
        return arrayValuesType;
    }
}