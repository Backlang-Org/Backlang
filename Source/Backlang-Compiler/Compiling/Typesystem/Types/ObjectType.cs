using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang_Compiler.Compiling.Typesystem.Types;

public sealed class ObjectType : IType
{
    public AttributeMap Attributes => new AttributeMap();
    public IReadOnlyList<IType> BaseTypes => null;
    public IReadOnlyList<IField> Fields => null;
    public QualifiedName FullName => new QualifiedName("object");
    public IReadOnlyList<IGenericParameter> GenericParameters => null;
    public IReadOnlyList<IMethod> Methods => null;
    public UnqualifiedName Name => FullName.FullyUnqualifiedName;
    public IReadOnlyList<IType> NestedTypes => null;
    public TypeParent Parent => new(new DescribedAssembly(new QualifiedName("System")));
    public IReadOnlyList<IProperty> Properties => null;
}