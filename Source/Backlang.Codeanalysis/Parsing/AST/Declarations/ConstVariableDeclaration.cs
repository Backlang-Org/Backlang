namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class ConstVariableDeclaration : VariableDeclarationStatement, IParsePoint<SyntaxNode>
{
    public ConstVariableDeclaration(string name, TypeLiteral? type, Expression? value) : base(name, type, false, value)
    {
    }

    public new static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var decl = (VariableDeclarationStatement)VariableDeclarationStatement.Parse(iterator, parser);

        return new GlobalVariableDeclaration(decl.Name, decl.Type, decl.Value);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}