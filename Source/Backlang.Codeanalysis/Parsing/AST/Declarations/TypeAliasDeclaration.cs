using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeAliasDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //type int = i32;
        var typeAliasDeclaration = new TypeAliasDeclaration();
        var aliasName = LNode.Id(iterator.NextToken().Text);

        iterator.Match(TokenType.EqualsToken);

        var toAlias = LNode.Id(iterator.NextToken().Text);

        iterator.Match(TokenType.Semicolon);

        return LNode.Call(CodeSymbols.Alias, LNode.List(aliasName, toAlias));
    }
}