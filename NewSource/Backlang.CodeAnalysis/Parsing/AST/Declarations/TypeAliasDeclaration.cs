using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeAliasDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // using <expression> as <identifier>
        var keywordToken = iterator.Prev;
        var expr = Expression.Parse(parser); // because 'as' is binary so i32 as int resolves to as(i32, int)

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.Using(expr).WithRange(keywordToken, iterator.Prev);
    }
}