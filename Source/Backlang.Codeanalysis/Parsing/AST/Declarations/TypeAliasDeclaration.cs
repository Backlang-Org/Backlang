namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class TypeAliasDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public string AliasName { get; set; }
    public string ToAlias { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        //type int = i32;
        var typeAliasDeclaration = new TypeAliasDeclaration();
        typeAliasDeclaration.AliasName = iterator.NextToken().Text;

        iterator.Match(TokenType.EqualsToken);

        typeAliasDeclaration.ToAlias = iterator.NextToken().Text;

        iterator.Match(TokenType.Semicolon);

        return typeAliasDeclaration;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}