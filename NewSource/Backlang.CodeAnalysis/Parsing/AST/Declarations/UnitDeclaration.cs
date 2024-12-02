using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class UnitDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.UnitDeclaration(nameToken).WithRange(keywordToken, nameToken);
    }
}