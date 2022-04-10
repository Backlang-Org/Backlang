using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Backlang_Compiler.Compiling.Stages;

namespace Backlang_Compiler.Compiling.Typesystem.Types;

public class VoidType : IType
{
    public AttributeMap Attributes => AttributeMap.Empty;
    public IReadOnlyList<IType> BaseTypes => null;
    public IReadOnlyList<IField> Fields => null;
    public QualifiedName FullName => new QualifiedName("void");
    public IReadOnlyList<IGenericParameter> GenericParameters => null;
    public IReadOnlyList<IMethod> Methods => null;
    public UnqualifiedName Name => FullName.Qualifier;
    public IReadOnlyList<IType> NestedTypes => null;
    public TypeParent Parent => TypeParent.Nothing;
    public IReadOnlyList<IProperty> Properties => null;
}
