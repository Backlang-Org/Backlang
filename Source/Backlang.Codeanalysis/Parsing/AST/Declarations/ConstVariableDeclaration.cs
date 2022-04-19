using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class ConstVariableDeclaration : VariableDeclarationStatement, IParsePoint<LNode>
{
    public ConstVariableDeclaration(string name, TypeLiteral? type, Expression? value) : base(name, type, isMutable: false, value)
    {
    }

    public new static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var decl = (VariableDeclarationStatement)VariableDeclarationStatement.Parse(iterator, parser);

        return new GlobalVariableDeclaration(decl.Name, decl.Type, decl.Value);
    }
}