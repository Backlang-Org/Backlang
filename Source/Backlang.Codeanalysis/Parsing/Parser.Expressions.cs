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
        var token = Iterator.Current;
        var type = token.Type;
        if (parsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return parsePoints[type](Iterator, this).WithRange(token, Iterator.Current);
        }
        else
        {
            return Invalid($"Unknown Expression. Expected String, Number, Boolean, {string.Join(", ",
                parsePoints.Keys)}");
        }
    }

    private LNode ParseBinNumber()
    {
        var valueToken = Iterator.NextToken();
        var chars = valueToken.Text.ToCharArray().Reverse().ToArray();

        int result = 0;
        for (int i = 0; i < valueToken.Text.Length; i++)
        {
            if (chars[i] == '0') { continue; }

            result += (int)Math.Pow(2, i);
        }

        return LNode.Call(CodeSymbols.Int32, LNode.List(SyntaxTree.Factory.Literal(result).WithStyle(NodeStyle.BinaryLiteral)))
            .WithRange(Iterator.Prev);
    }

    private LNode ParseBooleanLiteral(bool value)
    {
        Iterator.NextToken();

        return LNode.Call(CodeSymbols.Bool, LNode.List(SyntaxTree.Factory.Literal(value)))
            .WithRange(Iterator.Prev);
    }

    private LNode ParseChar()
    {
        return LNode.Call(CodeSymbols.Char,
            LNode.List(SyntaxTree.Factory.Literal(Iterator.NextToken().Text[0]))).WithRange(Iterator.Prev);
    }

    private LNode ParseHexNumber()
    {
        var valueToken = Iterator.NextToken();

        var parseSuccess = ParseHelpers.TryParseHex(valueToken.Text, out var result);

        if (!parseSuccess)
        {
            return LNode.Missing;
        }

        return LNode.Call(CodeSymbols.Int32,
            LNode.List(SyntaxTree.Factory.Literal(int.Parse(valueToken.Text, NumberStyles.HexNumber))))
            .WithRange(Iterator.Prev);
    }

    private LNode ParseNumber()
    {
        var text = (UString)Iterator.NextToken().Text;

        LNode result;
        if (text.Contains('.'))
        {
            var value = ParseHelpers.TryParseDouble(ref text, 10, ParseNumberFlag.SkipUnderscores);

            result = SyntaxTree.Factory.Literal(value).WithRange(Iterator.Prev);
        }
        else
        {
            var success = ParseHelpers.TryParseInt(ref text, out int value, 10, ParseNumberFlag.SkipUnderscores);

            if (!success)
            {
                result = LNode.Missing;
            }
            else
            {
                result = SyntaxTree.Factory.Literal(value).WithRange(Iterator.Prev);
            }
        }

        if (Iterator.Current.Type == TokenType.Identifier)
        {
            if (_lits.ContainsKey(Iterator.Current.Text.ToLower()))
            {
                result = LNode.Call(_lits[Iterator.Current.Text.ToLower()],
                    LNode.List(result)).WithRange(Iterator.Prev, Iterator.Current);
            }
            else
            {
                Messages.Add(Message.Error(Document, $"Unknown Literal {Iterator.Current.Text}",
                    Iterator.Current.Line, Iterator.Current.Column));

                result = LNode.Missing;
            }

            Iterator.NextToken();
        }
        else if (result.Value is double)
        {
            result = LNode.Call(Symbols.Float64, LNode.List(result)).WithRange(result.Range);
        }
        else
        {
            result = LNode.Call(CodeSymbols.Int32, LNode.List(result)).WithRange(result.Range);
        }

        return result;
    }

    private LNode ParseString()
    {
        var valueToken = Iterator.NextToken();
        return LNode.Call(CodeSymbols.String, LNode.List(SyntaxTree.Factory.Literal(valueToken.Text)))
            .WithRange(valueToken);
    }
}