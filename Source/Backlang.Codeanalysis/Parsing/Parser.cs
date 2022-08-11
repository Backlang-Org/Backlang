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

    public Parser(SourceFile<StreamCharSource> document, List<Token> tokens, List<Message> messages) : base(document, tokens, messages)
    {
        AddDeclarationParsePoint<BitFieldDeclaration>(TokenType.Bitfield);
        AddDeclarationParsePoint<UnionDeclaration>(TokenType.Union);
        AddDeclarationParsePoint<ClassDeclaration>(TokenType.Class);
        AddDeclarationParsePoint<ConstructorDeclaration>(TokenType.Constructor);
        AddDeclarationParsePoint<DestructorDeclaration>(TokenType.Destructor);
        AddDeclarationParsePoint<DiscriminatedUnionDeclaration>(TokenType.Type);
        AddDeclarationParsePoint<EnumDeclaration>(TokenType.Enum);
        AddDeclarationParsePoint<FunctionDeclaration>(TokenType.Function);
        AddDeclarationParsePoint<MacroDeclaration>(TokenType.Macro);
        AddDeclarationParsePoint<InterfaceDeclaration>(TokenType.Interface);
        AddDeclarationParsePoint<ImplementationDeclaration>(TokenType.Implement);
        AddDeclarationParsePoint<ImportStatement>(TokenType.Import);
        AddDeclarationParsePoint<StructDeclaration>(TokenType.Struct);
        AddDeclarationParsePoint<ModuleDeclaration>(TokenType.Module);
        AddDeclarationParsePoint<TypeAliasDeclaration>(TokenType.Using);
        AddDeclarationParsePoint<MacroBlockDeclaration>(TokenType.Identifier);

        AddExpressionParsePoint<NameExpression>(TokenType.Identifier);
        AddExpressionParsePoint<GroupOrTupleExpression>(TokenType.OpenParen);
        AddExpressionParsePoint<MatchExpression>(TokenType.Match);
        AddExpressionParsePoint<DefaultExpression>(TokenType.Default);
        AddExpressionParsePoint<SizeOfExpression>(TokenType.SizeOf);
        AddExpressionParsePoint<NoneExpression>(TokenType.None);
        AddExpressionParsePoint<InitializerListExpression>(TokenType.OpenSquare);

        AddStatementParsePoint<ThrowStatement>(TokenType.Throw);
        AddStatementParsePoint<BreakStatement>(TokenType.Break);
        AddStatementParsePoint<ContinueStatement>(TokenType.Continue);
        AddStatementParsePoint<ReturnStatement>(TokenType.Return);
        AddStatementParsePoint<VariableStatement>(TokenType.Let);
        AddStatementParsePoint<SwitchStatement>(TokenType.Switch);
        AddStatementParsePoint<IfStatement>(TokenType.If);
        AddStatementParsePoint<WhileStatement>(TokenType.While);
        AddStatementParsePoint<TryStatement>(TokenType.Try);
        AddStatementParsePoint<ForStatement>(TokenType.For);
        AddStatementParsePoint<MacroBlockStatement>(TokenType.Identifier);
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

    public LNodeList InvokeDeclarationParsePoints(TokenType terminator = TokenType.EOF)
    {
        var body = new LNodeList();
        while (Iterator.Current.Type != terminator)
        {
            Annotation.TryParse(this, out var annotation);
            Modifier.TryParse(this, out var modifiers);

            var item = InvokeParsePoint(DeclarationParsePoints)?.PlusAttrs(annotation).PlusAttrs(modifiers);

            if (item != null)
            {
                body.Add(item);
            }
        }

        return body;
    }

    public LNode InvokeParsePoint(ParsePoints<LNode> parsePoints)
    {
        var type = Iterator.Current.Type;

        if (parsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return parsePoints[type](Iterator, this);
        }

        var range = new SourceRange(Document, Iterator.Current.Start, Iterator.Current.Text.Length);

        Messages.Add(Message.Error($"Expected {string.Join(", ", parsePoints.Keys)}, got '{Iterator.Current.Text}'", range));
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

        var body = InvokeDeclarationParsePoints();

        cu.Messages = Messages.Concat(Iterator.Messages).ToList();
        cu.Body = body;

        return cu;
    }
}