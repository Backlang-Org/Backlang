using Backlang_Compiler.Parsing.AST;
using Backlang_Compiler.Parsing.AST.Expressions;
using System.Globalization;

namespace Backlang_Compiler.Parsing;

public partial class Parser
{
    internal override Expression ParsePrimary()
    {
        return Current.Type switch
        {
            TokenType.StringLiteral => ParseString(),
            TokenType.OpenParen => ParseGroup(),
            TokenType.Number => ParseNumber(),
            TokenType.TrueLiteral => ParseBooleanLiteral(true),
            TokenType.FalseLiteral => ParseBooleanLiteral(false),
            _ => Invalid("Unknown Expression. Expected String, Group, Number, Boolean, Day, DayOfWeek, Now, Time or Identifier"),
        };
    }

    private Expression Invalid(string message)
    {
        Messages.Add(Message.Error(message, Current.Line, Current.Column));

        return new InvalidExpr();
    }

    private UnaryExpression ParseAt()
    {
        return new UnaryExpression(NextToken(), Expression.Parse(this), false);
    }

    private Expression ParseBooleanLiteral(bool value)
    {
        NextToken();

        return new LiteralNode(value);
    }

    private Expression ParseGroup()
    {
        Match(TokenType.OpenParen);

        var expr = Expression.Parse(this);

        Match(TokenType.CloseParen);

        return new GroupExpression(expr);
    }

    private Expression ParseNumber()
    {
        return new LiteralNode(double.Parse(NextToken().Text, CultureInfo.InvariantCulture));
    }

    private Expression ParseString()
    {
        return new LiteralNode(NextToken().Text);
    }
}