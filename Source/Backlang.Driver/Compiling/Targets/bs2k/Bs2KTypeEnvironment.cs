using Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;
using Furesoft.Core.CodeDom.Backends.CLR;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class Bs2KTypeEnvironment : TypeEnvironment
{
    public override SubtypingRules Subtyping => ClrSubtypingRules.Instance;

    public IAssembly Assembly { get; set; }

    public override IType Void => new VoidType(Assembly);

    public override IType Float32 => throw new NotImplementedException();

    public override IType Float64 => throw new NotImplementedException();

    public override IType String => throw new NotImplementedException();

    public override IType Char => throw new NotImplementedException();

    public override IType NaturalInt => throw new NotImplementedException();

    public override IType NaturalUInt => throw new NotImplementedException();

    public override IType Object => throw new NotImplementedException();

    public override IType TypeToken => throw new NotImplementedException();

    public override IType FieldToken => throw new NotImplementedException();

    public override IType MethodToken => throw new NotImplementedException();

    public override IType CapturedException => throw new NotImplementedException();

    public override bool TryMakeArrayType(IType elementType, int rank, out IType arrayType)
    {
        throw new NotImplementedException();
    }

    public override bool TryMakeSignedIntegerType(int sizeInBits, out IType integerType)
    {
        throw new NotImplementedException();
    }

    public override bool TryMakeUnsignedIntegerType(int sizeInBits, out IType integerType)
    {
        throw new NotImplementedException();
    }
}