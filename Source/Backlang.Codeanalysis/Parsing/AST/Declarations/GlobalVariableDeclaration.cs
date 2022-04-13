namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class GlobalVariableDeclaration : VariableDeclarationStatement, IParsePoint<SyntaxNode>
{
    public GlobalVariableDeclaration(string name, TypeLiteral? type, Expression? value) : base(name, type, isMutable: true, value)
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