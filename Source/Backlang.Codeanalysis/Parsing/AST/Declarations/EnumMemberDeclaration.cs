namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class EnumMemberDeclaration : SyntaxNode
{
    public string Name { get; set; }
    public Expression? Value { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
