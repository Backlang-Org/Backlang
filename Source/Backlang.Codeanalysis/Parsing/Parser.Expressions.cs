using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using System.Globalization;

namespace Backlang.Codeanalysis.Parsing;

public sealed partial class Parser
{
    internal override Expression ParsePrimary(ParsePoints<Expression> parsePoints = null)
    {
        if (parsePoints == null)
        {
            parsePoints = ExpressionParsePoints;
        }

        return Iterator.Current.Type switch
        {
            TokenType.StringLiteral => ParseString(),
            TokenType.Number => ParseNumber(),
            TokenType.HexNumber => ParseHexNumber(),
            TokenType.BinNumber => ParseBinNumber(),
            TokenType.TrueLiteral => ParseBooleanLiteral(true),
            TokenType.FalseLiteral => ParseBooleanLiteral(false),
            _ => InvokeExpressionParsePoint(parsePoints),
        };
    }

    private Expression Invalid(string message)
    {
        Messages.Add(Message.Error(Document, message, Iterator.Current.Line, Iterator.Current.Column));

        return new InvalidExpr();
    }

    private Expression InvokeExpressionParsePoint(ParsePoints<Expression> parsePoints)
    {
        var type = Iterator.Current.Type;
        if (parsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return parsePoints[type](Iterator, this);
        }
        else
        {
            return Invalid($"Unknown Expression. Expected String, Number, Boolean, {string.Join(",", parsePoints.Keys)}");
        }
    }

    private Expression ParseBinNumber()
    {
        var valueToken = Iterator.NextToken();
        var chars = valueToken.Text.ToCharArray().Reverse().ToArray();

        long result = 0;
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

    private Expression ParseHexNumber()
    {
        var valueToken = Iterator.NextToken();

        return new LiteralNode(int.Parse(valueToken.Text, NumberStyles.HexNumber));
    }

    private Expression ParseNumber()
    {
        var valueToken = Iterator.NextToken();

        object value = long.Parse(valueToken.Text, CultureInfo.InvariantCulture);

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