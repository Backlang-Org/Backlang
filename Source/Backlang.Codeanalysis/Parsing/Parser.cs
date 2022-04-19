using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Expressions.Match;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public sealed partial class Parser : Core.BaseParser<Lexer, Parser>
{
    public readonly ParsePoints<LNode> DeclarationParsePoints = new();
    public readonly ParsePoints<LNode> ExpressionParsePoints = new();
    public readonly ParsePoints<LNode> StatementParsePoints = new();

    public Parser(SourceDocument document, List<Token> tokens, List<Message> messages) : base(document, tokens, messages)
    {
        AddDeclarationParsePoint<EnumDeclaration>(TokenType.Enum);
        AddDeclarationParsePoint<FunctionDeclaration>(TokenType.Function);
        AddDeclarationParsePoint<StructDeclaration>(TokenType.Struct);
        AddDeclarationParsePoint<BitFieldDeclaration>(TokenType.Bitfield);
        AddDeclarationParsePoint<TypeAliasDeclaration>(TokenType.Type);
        AddDeclarationParsePoint<GlobalVariableDeclaration>(TokenType.Global);
        AddDeclarationParsePoint<ConstVariableDeclaration>(TokenType.Const);
        AddDeclarationParsePoint<ImplementationDeclaration>(TokenType.ImplementationKeyword);

        AddExpressionParsePoint<NameExpression>(TokenType.Identifier);
        AddExpressionParsePoint<NameOfExpression>(TokenType.NameOf);
        AddExpressionParsePoint<GroupExpression>(TokenType.OpenParen);
        AddExpressionParsePoint<MatchExpression>(TokenType.Match);
        AddExpressionParsePoint<DefaultExpression>(TokenType.Default);
        AddExpressionParsePoint<SizeOfExpression>(TokenType.SizeOf);
        AddExpressionParsePoint<NoneExpression>(TokenType.None);
        AddExpressionParsePoint<InitializerListExpression>(TokenType.OpenSquare);

        //AddStatementParsePoint<AssemblerBlockStatement>(TokenType.Asm);
        AddStatementParsePoint<VariableDeclarationStatement>(TokenType.Declare);
        AddStatementParsePoint<IfStatement>(TokenType.If);
        AddStatementParsePoint<WhileStatement>(TokenType.While);
        AddStatementParsePoint<ForStatement>(TokenType.For);
    }

    public void AddDeclarationParsePoint<T>(TokenType type)
                where T : IParsePoint<LNode>
    {
        DeclarationParsePoints.Add(type, T.Parse);
    }

    public void AddExpressionParsePoint<T>(TokenType type)
            where T : IParsePoint<LNode>
    {
        ExpressionParsePoints.Add(type, T.Parse);
    }

    public void AddStatementParsePoint<T>(TokenType type)
        where T : IParsePoint<LNode>
    {
        StatementParsePoints.Add(type, T.Parse);
    }

    public LNode InvokeParsePoint(ParsePoints<LNode> parsePoints)
    {
        var type = Iterator.Current.Type;

        if (parsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return parsePoints[type](Iterator, this);
        }

        Messages.Add(Message.Error(Document, $"Expected {string.Join(",", parsePoints.Keys)}, got '{Iterator.Current.Text}'", Iterator.Current.Line, Iterator.Current.Column));
        Iterator.NextToken();

        return default;
    }

    public LNode InvokeStatementParsePoint()
    {
        var type = Iterator.Current.Type;

        if (StatementParsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return StatementParsePoints[type](Iterator, this);
        }

        return ExpressionStatement.Parse(Iterator, this);
    }

    protected override CompilationUnit Start()
    {
        var cu = new CompilationUnit();
        while (Iterator.Current.Type != (TokenType.EOF))
        {
            cu.Body.Add(InvokeParsePoint(DeclarationParsePoints));
        }

        cu.Messages = Messages.Concat(Iterator.Messages).ToList();

        return cu;
    }
}