using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using System.Globalization;

namespace Backlang.Codeanalysis.Parsing;

public partial class Parser
{
    internal override Expression ParsePrimary()
    {
        return Iterator.Current.Type switch
        {
            TokenType.StringLiteral => ParseString(),
            TokenType.OpenParen => ParseGroup(),
            TokenType.Number => ParseNumber(),
            TokenType.HexNumber => ParseHexNumber(),
            TokenType.BinNumber => ParseBinNumber(),
            TokenType.TrueLiteral => ParseBooleanLiteral(true),
            TokenType.FalseLiteral => ParseBooleanLiteral(false),
            TokenType.Identifier => ParseNameExpression(),
            _ => Invalid("Unknown Expression. Expected String, Group, Number, Boolean or Identifier"),
        };
    }

    private Expression Invalid(string message)
    {
        Messages.Add(Message.Error(message, Iterator.Current.Line, Iterator.Current.Column));

        return new InvalidExpr();
    }

    private Expression ParseBinNumber()
    {
        var valueToken = Iterator.NextToken();
        var chars = valueToken.Text.ToCharArray().Reverse().ToArray();

        int result = 0;
        for (int i = 0; i < valueToken.Text.Length; i++)
        {
            if (chars[i] == '0') { continue; }

            result += (int)Math.Pow(2, i);
        }

        return new LiteralNode(result);
    }

    private Expression ParseBooleanLiteral(bool value)
    {
        Iterator.NextToken();

        return new LiteralNode(value);
    }

    private Expression ParseGroup()
    {
        Iterator.Match(TokenType.OpenParen);

        var expr = Expression.Parse(this);

        Iterator.Match(TokenType.CloseParen);

        return new GroupExpression(expr);
    }

    private Expression ParseHexNumber()
    {
        var valueToken = Iterator.NextToken();

        return new LiteralNode(int.Parse(valueToken.Text, NumberStyles.HexNumber));
    }

    private Expression ParseNameExpression()
    {
        var token = Iterator.NextToken();
        return new NameExpression(token.Text, token.Line, token.Column);
    }

    private Expression ParseNumber()
    {
        var valueToken = Iterator.NextToken();

        object value = int.Parse(valueToken.Text, CultureInfo.InvariantCulture);

        if (value == null)
        {
            value = double.Parse(valueToken.Text, CultureInfo.InvariantCulture);
        }

        return new LiteralNode(value);
    }

    private Expression ParseString()
    {
        return new LiteralNode(Iterator.NextToken().Text);
    }
}