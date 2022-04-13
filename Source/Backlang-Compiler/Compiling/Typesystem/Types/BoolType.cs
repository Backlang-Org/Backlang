using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Backlang_Compiler.Compiling.Typesystem.Types;

public sealed class BoolType : IType
{
    public AttributeMap Attributes => new AttributeMap();
    public IReadOnlyList<IType> BaseTypes => null;
    public IReadOnlyList<IField> Fields => null;
    public QualifiedName FullName => new QualifiedName("bool");
    public IReadOnlyList<IGenericParameter> GenericParameters => null;
    public IReadOnlyList<IMethod> Methods => null;
    public UnqualifiedName Name => FullName.FullyUnqualifiedName;
    public IReadOnlyList<IType> NestedTypes => null;
    public TypeParent Parent => TypeParent.Nothing;
    public IReadOnlyList<IProperty> Properties => null;
}
