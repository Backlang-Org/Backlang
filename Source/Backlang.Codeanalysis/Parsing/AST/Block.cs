using Backlang.Codeanalysis.Parsing.AST;
namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Block : SyntaxNode
{
    public Block(List<SyntaxNode> body)
    {
        Body = body;
    }

    public Block()
    {
        Body = new List<SyntaxNode>();
    }

    public List<SyntaxNode> Body { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString()
    {
        return string.Join("\n", Body);
    }
}
