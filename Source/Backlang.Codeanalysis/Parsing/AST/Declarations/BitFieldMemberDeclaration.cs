using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class BitFieldMemberDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        string name = null;

        if (iterator.Current.Type == TokenType.Identifier)
        {
            name = iterator.Current.Text;

            iterator.NextToken();
        }
        else
        {
            name = iterator.NextToken().Text;
        }

        iterator.Match(TokenType.EqualsToken);

        var value = Expression.Parse(parser);

        if (!value[0].HasValue)
        {
            iterator.Messages.Add(Message.Error("Bitfield member declaration only allows literals", value.Range));
        }

        return SyntaxTree.Factory.Tuple(LNode.Id(name), value);
    }
}