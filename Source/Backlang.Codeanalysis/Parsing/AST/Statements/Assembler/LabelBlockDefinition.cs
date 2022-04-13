namespace Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

public sealed class LabelBlockDefinition : AssemblerBodyNode
{
    public List<AssemblerBodyNode> Body { get; set; }
    public string Name { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}