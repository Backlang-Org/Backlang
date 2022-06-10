using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class ImportStatement : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //import <identifier>
        //import <identifier>.<identifier>
        var keywordToken = iterator.Prev;
        var tree = SyntaxTree.Import(Expression.Parse(parser));

        iterator.Match(TokenType.Semicolon);

        return tree;
    }
}