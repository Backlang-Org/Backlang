using Backlang.Codeanalysis.Parsing.AST.Expressions;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class ArrayAccessExpression : Expression
{
    public List<Expression> Indices { get; set; } = new();
    public NameExpression Name { get; set; }
}