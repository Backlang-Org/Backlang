namespace Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

public sealed class Instruction : AssemblerBodyNode
{
    public List<Expression> Arguments { get; set; } = new();
    public string OpCode { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}