namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class AssignmentStatement : Statement
{
    public AssignmentStatement(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; set; }
    public Expression Value { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}