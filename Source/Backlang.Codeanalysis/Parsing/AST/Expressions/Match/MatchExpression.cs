namespace Backlang.Codeanalysis.Parsing.AST.Expressions.Match;

public sealed class MatchExpression : Expression, IParsePoint<Expression>
{
    /*
	 * match a with
		12 => 13,
		i32 num => num + 2,
		_ => 0 + 4;
	*/

    public Expression MatchArgument { get; set; }

    public List<MatchRule> Rules { get; set; } = new();

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        MatchExpression result = new MatchExpression();
        result.MatchArgument = Parse(parser);

        iterator.Match(TokenType.With);

        while (iterator.Current.Type != TokenType.Semicolon)
        {
            result.Rules.Add(MatchRule.Parse(iterator, parser));

            if (iterator.Current.Type == TokenType.Semicolon)
            {
                break;
            }
            else
            {
                iterator.Match(TokenType.Comma);
            }
        }

        return result;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}