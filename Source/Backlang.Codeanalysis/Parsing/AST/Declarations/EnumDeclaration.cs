using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class EnumDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var declaration = new EnumDeclaration();

        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        var members = new LNodeList();

        while (iterator.Current.Type != (TokenType.CloseCurly))
        {
            var memberNameToken = iterator.Current;
            LNode value = LNode.Missing;

            iterator.NextToken();

            if (iterator.Current.Type == TokenType.EqualsToken)
            {
                iterator.NextToken();

                value = parser.ParsePrimary();
            }

            if (iterator.Current.Type != TokenType.CloseCurly)
            {
                iterator.Match(TokenType.Comma);
            }

            members.Add(SyntaxTree.Factory.Var(LNode.Missing, LNode.Id(memberNameToken.Text), value));
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Enum(LNode.Id(nameToken.Text), members);
    }
}