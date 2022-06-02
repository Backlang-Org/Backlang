using Backlang.Codeanalysis.Parsing.AST;
using Loyc;
using Loyc.Syntax;
using System.Globalization;

namespace Backlang.Codeanalysis.Parsing;

public sealed partial class Parser
{
    private Dictionary<string, Symbol> _lits = new() {
        {"ub", CodeSymbols.UInt8},
        {"us", CodeSymbols.UInt16},
        {"u", CodeSymbols.UInt32},
        {"ui", CodeSymbols.UInt32},
        {"ul", CodeSymbols.UInt64},
        {"b", CodeSymbols.Int8},
        {"s", CodeSymbols.Int16},
        {"l", CodeSymbols.Int64},
        {"h", Symbols.Float16},
        {"f", Symbols.Float32},
        {"d", Symbols.Float64},
    };

    internal override LNode ParsePrimary(ParsePoints<LNode> parsePoints = null)
    {
        if (parsePoints == null)
        {
            parsePoints = ExpressionParsePoints;
        }

        return Iterator.Current.Type switch
        {
            TokenType.StringLiteral => ParseString(),
            TokenType.CharLiteral => ParseChar(),
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

    private LNode ParseChar()
    {
        return SyntaxTree.Factory.Literal(Iterator.NextToken().Text[0]);
    }

    private LNode ParseHexNumber()
    {
        var valueToken = Iterator.NextToken();

        return SyntaxTree.Unary(CodeSymbols.Int32, SyntaxTree.Factory.Literal(int.Parse(valueToken.Text, NumberStyles.HexNumber)));
    }

    private LNode ParseNumber()
    {
        var text = Iterator.NextToken().Text;

        LNode result;
        if (text.Contains('.'))
        {
            result = SyntaxTree.Factory.Literal(double.Parse(text, CultureInfo.InvariantCulture));
        }
        else
        {
            result = SyntaxTree.Factory.Literal(int.Parse(text));
        }

        if (Iterator.Current.Type == TokenType.Identifier)
        {
            if (_lits.ContainsKey(Iterator.Current.Text.ToLower()))
            {
                result = SyntaxTree.Unary(_lits[Iterator.Current.Text.ToLower()], result);
            }
            else
            {
                Messages.Add(Message.Error(Document, $"Unknown Literal {Iterator.Current.Text}", Iterator.Current.Line, Iterator.Current.Column));
                result = LNode.Missing;
            }

            Iterator.NextToken();
        } else if (result.Value is double)
        {
            result = SyntaxTree.Unary(Symbols.Float64, result);
        } else
        {
            result = SyntaxTree.Unary(CodeSymbols.Int32, result);
        }
        
        return result;
    }

    private LNode ParseString()
    {
        return SyntaxTree.Factory.Literal(Iterator.NextToken().Text);
    }
}