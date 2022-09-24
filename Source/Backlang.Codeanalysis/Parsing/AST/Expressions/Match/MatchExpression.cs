using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions.Match;

public sealed class MatchExpression : IParsePoint
{
    /*
	 * match a with
		12 => 13,
		i32 num => num + 2,
		_ => 0 + 4;
	*/

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var matchArgument = Expression.Parse(parser);

        iterator.Match(TokenType.With);

        var conditions = new LNodeList();

        while (iterator.Current.Type != TokenType.Semicolon)
        {
            conditions.Add(MatchRule.Parse(iterator, parser));

            if (iterator.Current.Type == TokenType.Semicolon)
            {
                break;
            }
            else
            {
                iterator.Match(TokenType.Comma);
            }
        }

        return SyntaxTree.Factory.Call(LNode.Id(Symbols.Match), matchArgument).WithAttrs(conditions);
    }
}