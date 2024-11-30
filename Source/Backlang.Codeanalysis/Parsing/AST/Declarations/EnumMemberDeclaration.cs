using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class EnumMemberDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        _ = Annotation.TryParse(parser, out var annotations);

        var memberNameToken = iterator.Match(TokenType.Identifier);
        LNode value = LNode.Missing;

        if (iterator.ConsumeIfMatch(TokenType.EqualsToken))
        {
            value = parser.ParsePrimary();
        }

        return SyntaxTree.Factory.Var(LNode.Missing, LNode.Id(memberNameToken.Text), value).PlusAttrs(annotations);
    }
}