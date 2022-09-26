using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class EnumDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        var members = ParsingHelpers.ParseSeperated(parser, _ => {
            Annotation.TryParse(parser, out var annotations);

            var memberNameToken = iterator.Match(TokenType.Identifier);
            LNode value = LNode.Missing;

            if (iterator.ConsumeIfMatch(TokenType.EqualsToken))
            {
                value = parser.ParsePrimary();
            }

            return SyntaxTree.Factory.Var(LNode.Missing, LNode.Id(memberNameToken.Text), value).PlusAttrs(annotations);
        }, TokenType.CloseCurly);

        return SyntaxTree.Enum(LNode.Id(nameToken.Text), members).WithRange(keywordToken, iterator.Prev);
    }
}