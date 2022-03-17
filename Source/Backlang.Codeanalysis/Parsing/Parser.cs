using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Expressions.Match;
using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang.Codeanalysis.Parsing;

public partial class Parser : BaseParser<SyntaxNode, Lexer, Parser>
{
    public readonly ParsePoints<SyntaxNode> DeclarationParsePoints = new();
    public readonly ParsePoints<Expression> ExpressionParsePoints = new();
    public readonly ParsePoints<Statement> StatementParsePoints = new();

    public Parser(SourceDocument document, List<Token> tokens, List<Message> messages) : base(document, tokens, messages)
    {
        AddDeclarationParsePoint<EnumDeclaration>(TokenType.Enum);
        AddDeclarationParsePoint<FunctionDeclaration>(TokenType.Function);
        AddDeclarationParsePoint<StructDeclaration>(TokenType.Struct);
        AddDeclarationParsePoint<BitFieldDeclaration>(TokenType.Bitfield);
        AddDeclarationParsePoint<RegisterDeclaration>(TokenType.Register);
        AddDeclarationParsePoint<TypeAliasDeclaration>(TokenType.Type);

        AddExpressionParsePoint<NameExpression>(TokenType.Identifier);
        AddExpressionParsePoint<GroupExpression>(TokenType.OpenParen);
        AddExpressionParsePoint<MatchExpression>(TokenType.Match);
        AddExpressionParsePoint<DefaultExpression>(TokenType.Default);
        AddExpressionParsePoint<SizeOfExpression>(TokenType.SizeOf);
        AddExpressionParsePoint<NoneExpression>(TokenType.None);
        AddExpressionParsePoint<InitializerListExpression>(TokenType.OpenSquare);

        AddStatementParsePoint<VariableDeclarationStatement>(TokenType.Declare);
    }

    public void AddDeclarationParsePoint<T>(TokenType type)
                where T : IParsePoint<SyntaxNode>
    {
        DeclarationParsePoints.Add(type, T.Parse);
    }

    public void AddExpressionParsePoint<T>(TokenType type)
            where T : IParsePoint<Expression>
    {
        ExpressionParsePoints.Add(type, T.Parse);
    }

    public void AddStatementParsePoint<T>(TokenType type)
        where T : IParsePoint<Statement>
    {
        StatementParsePoints.Add(type, T.Parse);
    }

    public T InvokeParsePoint<T>(ParsePoints<T> parsePoints)
    {
        var type = Iterator.Current.Type;

        if (parsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return parsePoints[type](Iterator, this);
        }
        else
        {
            Messages.Add(Message.Error($"Expected {string.Join(",", parsePoints.Keys)}, got '{Iterator.Current.Text}'", Iterator.Current.Line, Iterator.Current.Column));
            Iterator.NextToken();

            return default;
        }
    }

    public Statement InvokeStatementParsePoint()
    {
        var type = Iterator.Current.Type;

        if (StatementParsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return StatementParsePoints[type](Iterator, this);
        }
        else
        {
            return ExpressionStatement.Parse(Iterator, this);
        }
    }

    protected override SyntaxNode Start()
    {
        var cu = new CompilationUnit();
        while (Iterator.Current.Type != (TokenType.EOF))
        {
            cu.Body.Body.Add(InvokeParsePoint(DeclarationParsePoints));
        }

        cu.Messages = Messages.Concat(Iterator.Messages).ToList();

        return cu;
    }
}