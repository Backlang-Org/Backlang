using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Core.Attributes;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using System.Reflection;

namespace Backlang.Codeanalysis.Parsing;

public class Expression : SyntaxNode
{
    public static List<OperatorInfo> Operators = new List<OperatorInfo>();

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

    public static Expression Parse<TNode, TLexer, TParser>(
        BaseParser<TNode, TLexer, TParser> parser,
        ParsePoints<Expression> parsePoints = null,
        int parentPrecedence = 0)

        where TParser : BaseParser<TNode, TLexer, TParser>
        where TLexer : BaseLexer, new()
    {
        Expression left;
        var unaryOperatorPrecedence = GetUnaryOperatorPrecedence(parser.Iterator.Current.Type);

        if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            Token? operatorToken = parser.Iterator.NextToken();
            Expression? operand = Parse(parser, parsePoints, unaryOperatorPrecedence + 1);

            left = new UnaryExpression(operatorToken, operand, false);
        }
        else
        {
            left = parser.ParsePrimary(parsePoints);

            if (IsPostUnary(parser.Iterator.Current.Type))
            {
                Token? operatorToken = parser.Iterator.NextToken();

                left = new UnaryExpression(operatorToken, left, true);
            }
        }

        while (true)
        {
            var precedence = GetBinaryOperatorPrecedence(parser.Iterator.Current.Type);
            if (precedence == 0 || precedence <= parentPrecedence)
                break;

            var operatorToken = parser.Iterator.NextToken();
            var right = Parse(parser, parsePoints, precedence);

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    public static List<Expression> ParseList<TNode, TLexer, TParser>(BaseParser<TNode, TLexer, TParser> parser, TokenType terminator,
            ParsePoints<Expression> parsePoints = null)

        where TParser : BaseParser<TNode, TLexer, TParser>
        where TLexer : BaseLexer, new()
    {
        var list = new List<Expression>();
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

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
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