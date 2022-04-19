namespace Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

public sealed class Instruction : AssemblerBodyNode
{
    public List<Expression> Arguments { get; set; } = new();
    public string OpCode { get; set; }
}