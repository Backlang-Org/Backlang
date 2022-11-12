using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Expressions.Match;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public sealed partial class Parser
{
    public readonly ParsePoints DeclarationParsePoints = new();
    public readonly ParsePoints ExpressionParsePoints = new();
    public readonly ParsePoints StatementParsePoints = new();

    public void InitParsePoints()
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
        AddDeclarationParsePoint<ImportDeclaration>(TokenType.Import);
        AddDeclarationParsePoint<StructDeclaration>(TokenType.Struct);
        AddDeclarationParsePoint<ModuleDeclaration>(TokenType.Module);
        AddDeclarationParsePoint<TypeAliasDeclaration>(TokenType.Using);
        AddDeclarationParsePoint<UnitDeclaration>(TokenType.Unit);
        AddDeclarationParsePoint<MacroBlockDeclaration>(TokenType.Identifier);

        AddExpressionParsePoint<NameExpression>(TokenType.Identifier);
        AddExpressionParsePoint<GroupOrTupleExpression>(TokenType.OpenParen);
        AddExpressionParsePoint<MatchExpression>(TokenType.Match);
        AddExpressionParsePoint<DefaultExpression>(TokenType.Default);
        AddExpressionParsePoint<SizeOfExpression>(TokenType.SizeOf);
        AddExpressionParsePoint<TypeofExpression>(TokenType.TypeOf);
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
        AddStatementParsePoint<DoWhileStatement>(TokenType.Do);
        AddStatementParsePoint<TryStatement>(TokenType.Try);
        AddStatementParsePoint<ForStatement>(TokenType.For);
        AddStatementParsePoint<MacroBlockStatement>(TokenType.Identifier);
    }

    public void AddDeclarationParsePoint<T>(TokenType type)
                where T : IParsePoint
    {
        DeclarationParsePoints.Add(type, T.Parse);
    }

    public void AddExpressionParsePoint<T>(TokenType type)
            where T : IParsePoint
    {
        ExpressionParsePoints.Add(type, T.Parse);
    }

    public void AddStatementParsePoint<T>(TokenType type)
        where T : IParsePoint
    {
        StatementParsePoints.Add(type, T.Parse);
    }

    public LNodeList InvokeDeclarationParsePoints(TokenType terminator = TokenType.EOF, ParsePoints parsePoints = null)
    {
        if (parsePoints == null) parsePoints = DeclarationParsePoints;

        var body = new LNodeList();
        while (Iterator.Current.Type != terminator)
        {
            Annotation.TryParse(this, out var annotation);
            Modifier.TryParse(this, out var modifiers);

            var item = InvokeParsePoint(parsePoints)?.PlusAttrs(annotation).PlusAttrs(modifiers);

            if (item != null)
            {
                body.Add(item);
            }
        }

        return body;
    }

    public LNode InvokeParsePoint(ParsePoints parsePoints)
    {
        var type = Iterator.Current.Type;

        if (parsePoints.ContainsKey(type))
        {
            Iterator.NextToken();

            return parsePoints[type](Iterator, this);
        }

        var range = new SourceRange(Document, Iterator.Current.Start, Iterator.Current.Text.Length);

        AddError(new(ErrorID.UnknownExpression, Iterator.Current.Text), range);

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
}