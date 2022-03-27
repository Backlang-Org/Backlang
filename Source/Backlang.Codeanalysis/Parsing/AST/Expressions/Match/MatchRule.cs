using Backlang.Codeanalysis.Parsing.AST.Expressions.Match.Rules;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions.Match;

public class MatchRule
{
    public Expression Result { get; set; }

    public static MatchRule Parse(TokenIterator iterator, Parser parser)
    {
        if (iterator.Current.Type == TokenType.Underscore) // _ => 0
        {
            iterator.NextToken();

            var defaultRule = new DefaultMatchRule();

            iterator.Match(TokenType.GoesTo);

            defaultRule.Result = Expression.Parse(parser);

            return defaultRule;
        }
        else if (iterator.Peek(1).Type == TokenType.GoesTo) //12 => 13
        {
            var simpleRule = new SimpleExpressionRule();
            simpleRule.Matcher = Expression.Parse(parser);

            iterator.Match(TokenType.GoesTo);

            simpleRule.Result = Expression.Parse(parser);

            return simpleRule;
        }
        else if (iterator.Current.IsOperator()) // > 12 => false
        {
            var condRule = new ConditionRule();

            condRule.OperatorToken = iterator.Current;
            iterator.NextToken();

            condRule.Condition = Expression.Parse(parser);

            iterator.Match(TokenType.GoesTo);

            condRule.Result = Expression.Parse(parser);

            return condRule;
        }
        else if (iterator.Current.Type == TokenType.Identifier && iterator.Peek(1).Type == TokenType.Identifier) //i32 num => num + 2
        {
            var namedRule = new TypeNameRule();
            namedRule.Type = iterator.NextToken().Text;
            namedRule.Name = iterator.NextToken().Text;

            iterator.Match(TokenType.GoesTo);

            namedRule.Result = Expression.Parse(parser);

            return namedRule;
        }
        else if (iterator.Current.Type == TokenType.Identifier) //i32 => 32
        {
            var typeRule = new TypeRule();
            typeRule.Type = iterator.NextToken().Text;

            iterator.Match(TokenType.GoesTo);

            typeRule.Result = Expression.Parse(parser);

            return typeRule;
        }

        return null;
    }
}