using Backlang.Codeanalysis.Core.Attributes;
using Loyc;
using Loyc.Syntax;
using System.Reflection;

namespace Backlang.Codeanalysis.Parsing;

public static class Expression
{
    public static readonly Dictionary<TokenType, int> BinaryOperators = new();
    public static readonly Dictionary<TokenType, int> PostUnaryOperators = new();
    public static readonly Dictionary<TokenType, int> PreUnaryOperators = new();

    static Expression()
    {
        var typeValues = (TokenType[])Enum.GetValues(typeof(TokenType));

        foreach (var op in typeValues)
        {
            var attributes = op.GetType().GetField(Enum.GetName(op)).GetCustomAttributes<OperatorInfoAttribute>(true);

            if (attributes != null && attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    if (attribute.IsUnary)
                    {
                        if (attribute.IsPostUnary)
                        {
                            PostUnaryOperators.Add(op, attribute.Precedence);
                        }
                        else
                        {
                            PreUnaryOperators.Add(op, attribute.Precedence);
                        }
                    }
                    else
                    {
                        BinaryOperators.Add(op, attribute.Precedence);
                    }
                }
            }
        }
    }

    public static int GetBinaryOperatorPrecedence(TokenType kind) => BinaryOperators.GetValueOrDefault(kind);

    public static LNode Parse(Parser parser, ParsePoints parsePoints = null, int parentPrecedence = 0)
    {
        LNode left;
        var unaryOperatorPrecedence = GetPreUnaryOperatorPrecedence(parser.Iterator.Current.Type);

        if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            var operatorToken = parser.Iterator.NextToken();

            var operand = Parse(parser, parsePoints, unaryOperatorPrecedence + 1);

            left = SyntaxTree.Unary(GSymbol.Get($"'{operatorToken.Text}"), operand).WithRange(operatorToken.Start, operand.Range.EndIndex).WithStyle(NodeStyle.PrefixNotation);
        }
        else
        {
            left = parser.ParsePrimary(parsePoints);

            if (IsPostUnary(parser.Iterator.Current.Type))
            {
                var operatorToken = parser.Iterator.NextToken();

                left = SyntaxTree.Unary(GSymbol.Get($"'{operatorToken.Text}"), left).WithRange(left.Range.StartIndex, operatorToken.End);
            }
        }

        while (true)
        {
            var precedence = GetBinaryOperatorPrecedence(parser.Iterator.Current.Type);
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                break;
            }

            var operatorToken = parser.Iterator.NextToken();
            var right = Parse(parser, parsePoints, precedence);

            left = SyntaxTree.Binary(GSymbol.Get($"'{operatorToken.Text}"), left, right).WithRange(left.Range.StartIndex, right.Range.StartIndex);
        }

        return left;
    }

    public static LNodeList ParseList(Parser parser, TokenType terminator,
            ParsePoints parsePoints = null)

    {
        if (parser.Iterator.IsMatch(terminator))
        {
            parser.Iterator.Match(terminator);
            return LNodeList.Empty;
        }

        var list = new LNodeList();
        do
        {
            list.Add(Expression.Parse(parser, parsePoints));
        } while (parser.Iterator.ConsumeIfMatch(TokenType.Comma));

        parser.Iterator.Match(terminator);
        return list;
    }

    private static int GetPreUnaryOperatorPrecedence(TokenType kind) => PreUnaryOperators.GetValueOrDefault(kind);

    private static bool IsPostUnary(TokenType kind) => PostUnaryOperators.ContainsKey(kind);
}