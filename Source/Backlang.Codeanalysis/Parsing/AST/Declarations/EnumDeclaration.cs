using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class EnumDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        var members = new LNodeList();

        while (iterator.Current.Type != (TokenType.CloseCurly))
        {
            var hasAnnotations = Annotation.TryParse(parser, out var annotations);

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

            members.Add(SyntaxTree.Factory.Var(LNode.Missing, LNode.Id(memberNameToken.Text), value).PlusAttrs(annotations));
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Enum(LNode.Id(nameToken.Text), members).WithRange(keywordToken, iterator.Prev);
    }
}