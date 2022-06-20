using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class UnionDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        var members = new LNodeList();

        do
        {
            members.Add(ParseUnionMember(parser));
        } while (!iterator.IsMatch(TokenType.EOF) && !iterator.IsMatch(TokenType.CloseCurly));

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Union(nameToken.Text, members).WithRange(keywordToken, iterator.Current);
    }

    private static LNode ParseUnionMember(Parser parser)
    {
        return VariableStatement.Parse(parser.Iterator, parser);
    }
}