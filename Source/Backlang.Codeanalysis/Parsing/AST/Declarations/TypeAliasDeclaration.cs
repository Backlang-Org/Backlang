using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeAliasDeclaration : IParsePoint<LNode>
{
    public string AliasName { get; set; }
    public string ToAlias { get; set; }

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //type int = i32;
        var typeAliasDeclaration = new TypeAliasDeclaration();
        typeAliasDeclaration.AliasName = iterator.NextToken().Text;

        iterator.Match(TokenType.EqualsToken);

        typeAliasDeclaration.ToAlias = iterator.NextToken().Text;

        iterator.Match(TokenType.Semicolon);

        return typeAliasDeclaration;
    }
}