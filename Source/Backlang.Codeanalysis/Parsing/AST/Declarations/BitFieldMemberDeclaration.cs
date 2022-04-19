using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class BitFieldMemberDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var member = new BitFieldMemberDeclaration();

        if (iterator.Current.Type == TokenType.Identifier)
        {
            member.Name = iterator.Current.Text;

            iterator.NextToken();
        }
        else
        {
            member.Name = iterator.NextToken().Text;
        }

        iterator.Match(TokenType.EqualsToken);

        member.Index = Expression.Parse(parser);

        return member;
    }
}