using Loyc.Syntax;
using System.Globalization;

namespace Backlang.Codeanalysis.Parsing;

public sealed partial class Parser
{
    internal override LNode ParsePrimary(ParsePoints<LNode> parsePoints = null)
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

    private LNode Invalid(string message)
    {
        Messages.Add(Message.Error(Document, message, Iterator.Current.Line, Iterator.Current.Column));

        return LNode.Call(CodeSymbols.Error, LNode.List(LNode.Literal(message)));
    }

    private LNode InvokeExpressionParsePoint(ParsePoints<LNode> parsePoints)
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

    private LNode ParseBinNumber()
    {
        var valueToken = Iterator.NextToken();
        var chars = valueToken.Text.ToCharArray().Reverse().ToArray();

        long result = 0;
        for (int i = 0; i < valueToken.Text.Length; i++)
        {
            if (chars[i] == '0') { continue; }

            result += (int)Math.Pow(2, i);
        }

        return SyntaxTree.Factory.Literal(result);
    }

    private LNode ParseBooleanLiteral(bool value)
    {
        Iterator.NextToken();

        return SyntaxTree.Factory.Literal(value);
    }

    private LNode ParseHexNumber()
    {
        var valueToken = Iterator.NextToken();

        return SyntaxTree.Factory.Literal(int.Parse(valueToken.Text, NumberStyles.HexNumber));
    }

    private LNode ParseNumber()
    {
        var valueToken = Iterator.NextToken();

        object value = long.Parse(valueToken.Text, CultureInfo.InvariantCulture);

        if (value == null)
        {
            value = double.Parse(valueToken.Text, CultureInfo.InvariantCulture);
        }

        return SyntaxTree.Factory.Literal(value);
    }

    private LNode ParseString()
    {
        return SyntaxTree.Factory.Literal(Iterator.NextToken().Text);
    }
}