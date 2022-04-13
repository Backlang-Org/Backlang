using Backlang_Compiler.Compiling.Stages;
using Backlang_Compiler.Compiling.Typesystem.Types;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang_Compiler.Compiling.Typesystem;

public sealed class BackTypeEnvironment : TypeEnvironment
{
    public override IType CapturedException => throw new NotImplementedException();
    public override IType Char => throw new NotImplementedException();
    public override IType FieldToken => throw new NotImplementedException();
    public override IType Float32 => throw new NotImplementedException();
    public override IType Float64 => throw new NotImplementedException();
    public override IType MethodToken => throw new NotImplementedException();
    public override IType NaturalInt => throw new NotImplementedException();
    public override IType NaturalUInt => throw new NotImplementedException();
    public override IType Object => throw new NotImplementedException();
    public override IType String => new StringType();
    public override SubtypingRules Subtyping => null;

    public override IType TypeToken => throw new NotImplementedException();
    public override IType Void => new VoidType();

    public override bool TryMakeArrayType(IType elementType, int rank, out IType arrayType)
    {
        arrayType = new ArrayType(rank, elementType);

        return true;
    }

    public override bool TryMakeSignedIntegerType(int sizeInBits, out IType integerType)
    {
        integerType = null;

        return false;
    }

    public override bool TryMakeUnsignedIntegerType(int sizeInBits, out IType integerType)
    {
        switch (sizeInBits)
        {
            case 1:
                integerType = new BoolType();
                break;

            case 8:
                integerType = new U8Type();
                break;

            case 16:
                integerType = new U16Type();
                break;

            case 32:
                integerType = new U32Type();
                break;

            default:
                integerType = new U32Type();
                break;
        }

        return true;
    }
}