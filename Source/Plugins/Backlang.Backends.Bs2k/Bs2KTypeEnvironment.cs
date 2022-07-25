using Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;
using Furesoft.Core.CodeDom.Backends.CLR;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class Bs2KTypeEnvironment : TypeEnvironment
{
    public Bs2KTypeEnvironment()
    {
        Assembly = new DescribedAssembly(new SimpleName("System").Qualify());

        ((DescribedAssembly)Assembly).AddType(Void);
        ((DescribedAssembly)Assembly).AddType(Boolean);
        ((DescribedAssembly)Assembly).AddType(String);
        ((DescribedAssembly)Assembly).AddType(Char);
        ((DescribedAssembly)Assembly).AddType(Object);

        ((DescribedAssembly)Assembly).AddType(Int8);
        ((DescribedAssembly)Assembly).AddType(Int16);
        ((DescribedAssembly)Assembly).AddType(Int32);
        ((DescribedAssembly)Assembly).AddType(Int64);

        ((DescribedAssembly)Assembly).AddType(UInt8);
        ((DescribedAssembly)Assembly).AddType(UInt16);
        ((DescribedAssembly)Assembly).AddType(UInt32);
        ((DescribedAssembly)Assembly).AddType(UInt64);
    }

    public override SubtypingRules Subtyping => ClrSubtypingRules.Instance;

    public IAssembly Assembly { get; set; }
    public override IType Void => new VoidType(Assembly);

    public override IType Float32 => new I32Type(Assembly); // ! ! ! FALLBACK - Change Later ! ! !

    public override IType Float64 => new I64Type(Assembly); // ! ! ! FALLBACK - Change Later ! ! !

    public override IType String => new StringType(Assembly);

    public override IType Char => new CharType(Assembly);

    public override IType NaturalInt => new I32Type(Assembly);

    public override IType NaturalUInt => new U32Type(Assembly);

    public override IType Object => new ObjectType(Assembly);

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
        integerType = sizeInBits switch
        {
            8 => new I8Type(Assembly),
            16 => new I16Type(Assembly),
            32 => new I32Type(Assembly),
            64 => new I64Type(Assembly),
            _ => null
        };

        return integerType != null;
    }

    public override bool TryMakeUnsignedIntegerType(int sizeInBits, out IType integerType)
    {
        integerType = sizeInBits switch
        {
            1 => new BooleanType(Assembly),
            8 => new U8Type(Assembly),
            16 => new U16Type(Assembly),
            32 => new U32Type(Assembly),
            64 => new U64Type(Assembly),
            _ => null
        };

        return integerType != null;
    }
}