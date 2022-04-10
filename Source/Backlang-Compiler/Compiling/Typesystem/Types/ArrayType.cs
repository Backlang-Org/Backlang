using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang_Compiler.Compiling.Typesystem.Types;

public class ArrayType : ContainerType
{
    public ArrayType(int rank, IType arrayType) : base(arrayType)
    {
        Rank = rank;
        Type = arrayType;
    }

    public new QualifiedName FullName => new QualifiedName(Type.Name + $"[{Rank}]");

    public new UnqualifiedName Name => FullName.FullyUnqualifiedName;

    public int Rank { get; set; }
    public IType Type { get; set; }

    public override ContainerType WithElementType(IType newElementType)
    {
        Type = newElementType;

        return this;
    }
}