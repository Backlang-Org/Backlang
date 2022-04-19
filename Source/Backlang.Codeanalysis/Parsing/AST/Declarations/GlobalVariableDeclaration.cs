using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class GlobalVariableDeclaration : VariableDeclarationStatement, IParsePoint<LNode>
{
    public GlobalVariableDeclaration(string name, TypeLiteral? type, Expression? value) : base(name, type, isMutable: true, value)
    {
    }

    public new static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var decl = (VariableDeclarationStatement)VariableDeclarationStatement.Parse(iterator, parser);

        return new GlobalVariableDeclaration(decl.Name, decl.Type, decl.Value);
    }
}