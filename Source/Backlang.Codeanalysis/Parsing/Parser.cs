using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang.Codeanalysis.Parsing;

public partial class Parser : BaseParser<SyntaxNode, Lexer, Parser>
{
    private readonly Dictionary<TokenType, Func<TokenIterator, Parser, SyntaxNode>> _declarationParsePoints = new();
    private readonly Dictionary<TokenType, Func<TokenIterator, Parser, Expression>> _expressionParsePoints = new();
    private readonly Dictionary<TokenType, Func<TokenIterator, Parser, Statement>> _statementParsePoints = new();

    public Parser(SourceDocument document, List<Token> tokens, List<Message> messages) : base(document, tokens, messages)
    {
        AddDeclarationParsePoint<EnumDeclaration>(TokenType.Enum);
        AddDeclarationParsePoint<FunctionDeclaration>(TokenType.Function);
        AddDeclarationParsePoint<StructDeclaration>(TokenType.Struct);

        AddStatementParsePoint<VariableDeclarationStatement>(TokenType.Declare);
    }

    public void AddDeclarationParsePoint<T>(TokenType type)
            where T : IParsePoint<SyntaxNode>
    {
        _declarationParsePoints.Add(type, T.Parse);
    }

    public void AddExpressionParsePoint<T>(TokenType type)
            where T : IParsePoint<Expression>
    {
        _expressionParsePoints.Add(type, T.Parse);
    }

    public void AddStatementParsePoint<T>(TokenType type)
        where T : IParsePoint<Statement>
    {
        _statementParsePoints.Add(type, T.Parse);
    }

    public Statement InvokeStatementParsePoint()
    {
        var type = Iterator.Current.Type;

        if (_statementParsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return _statementParsePoints[type](Iterator, this);
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
            var type = Iterator.Current.Type;

            if (_declarationParsePoints.ContainsKey(type))
            {
                Iterator.NextToken();

                cu.Body.Body.Add(_declarationParsePoints[type](Iterator, this));
            }
            else
            {
                Messages.Add(Message.Error($"Expected {string.Join(" or ", _declarationParsePoints.Keys)}, got '{Iterator.Current.Text}'", Iterator.Current.Line, Iterator.Current.Column));
                Iterator.NextToken();
            }
        }

        cu.Messages = Messages.Concat(Iterator.Messages).ToList();

        return cu;
    }
}