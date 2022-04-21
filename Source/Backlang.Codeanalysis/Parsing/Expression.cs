using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Core.Attributes;
using Loyc;
using Loyc.Syntax;
using System.Reflection;

namespace Backlang.Codeanalysis.Parsing;

public static class Expression
{
    public static IList<OperatorInfo> Operators = new List<OperatorInfo>();

    static Expression()
    {
        var typeValues = (TokenType[])Enum.GetValues(typeof(TokenType));

        foreach (var op in typeValues)
        {
            var attributes = op.GetType().GetField(Enum.GetName<TokenType>(op)).GetCustomAttributes<OperatorInfoAttribute>(true);

            if (attributes != null && attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    Operators.Add(new OperatorInfo(op, attribute.Precedence, attribute.IsUnary, attribute.IsPostUnary));
                }
            }
        }
    }

    public static LNode Parse<TLexer, TParser>(
        Core.BaseParser<TLexer, TParser> parser,
        ParsePoints<LNode> parsePoints = null,
        int parentPrecedence = 0)

        where TParser : Core.BaseParser<TLexer, TParser>
        where TLexer : BaseLexer, new()
    {
        LNode left;
        var unaryOperatorPrecedence = GetUnaryOperatorPrecedence(parser.Iterator.Current.Type);

        if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            Token? operatorToken = parser.Iterator.NextToken();
            LNode? operand = Parse(parser, parsePoints, unaryOperatorPrecedence + 1);

            left = SyntaxTree.Unary(GSymbol.Get("'suf" + operatorToken.Text), operand);
        }
        else
        {
            left = parser.ParsePrimary(parsePoints);

            if (IsPostUnary(parser.Iterator.Current.Type))
            {
                Token? operatorToken = parser.Iterator.NextToken();

                left = SyntaxTree.Unary(GSymbol.Get("'" + operatorToken.Text), left);
            }
        }

        while (true)
        {
            var precedence = GetBinaryOperatorPrecedence(parser.Iterator.Current.Type);
            if (precedence == 0 || precedence <= parentPrecedence)
                break;

            var operatorToken = parser.Iterator.NextToken();
            var right = Parse(parser, parsePoints, precedence);

            left = SyntaxTree.Binary(GSymbol.Get(operatorToken.Text), left, right);
        }

        return left;
    }

    public static LNodeList ParseList<TLexer, TParser>(Core.BaseParser<TLexer, TParser> parser, TokenType terminator,
            ParsePoints<LNode> parsePoints = null)

        where TParser : Core.BaseParser<TLexer, TParser>
        where TLexer : BaseLexer, new()
    {
        var list = new LNodeList();
        while (parser.Iterator.Current.Type != terminator) //ToDo: implement option to disallow empty list
        {
            list.Add(Expression.Parse(parser));

            if (parser.Iterator.Current.Type != terminator)
            {
                parser.Iterator.Match(TokenType.Comma);
            }
        }

        parser.Iterator.Match(terminator);

        return list;
    }

    private static int GetBinaryOperatorPrecedence(TokenType kind)
    {
        for (int i = 0; i < Operators.Count - 1; i++)
        {
            if (Operators[i].TokenType == kind && !Operators[i].IsUnary)
            {
                return Operators[i].Precedence;
            }
        }

        return 0;
    }

    private static int GetUnaryOperatorPrecedence(TokenType kind)
    {
        for (int i = 0; i < Operators.Count - 1; i++)
        {
            if (Operators[i].TokenType == kind && Operators[i].IsUnary)
            {
                return Operators[i].Precedence;
            }
        }

        return 0;
    }

    private static bool IsPostUnary(TokenType kind)
    {
        for (int i = 0; i < Operators.Count - 1; i++)
        {
            if (Operators[i].TokenType == kind && Operators[i].IsUnary)
            {
                return Operators[i].IsPostUnary;
            }
        }

        return false;
    }
}