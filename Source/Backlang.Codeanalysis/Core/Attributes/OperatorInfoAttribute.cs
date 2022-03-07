namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class OperatorInfoAttribute : Attribute
{
    public OperatorInfoAttribute(int precedence, bool isUnary, bool isPostUnary)
    {
        Precedence = precedence;
        IsUnary = isUnary;
        IsPostUnary = isPostUnary;
    }

    public bool IsPostUnary { get; set; }
    public bool IsUnary { get; set; }
    public int Precedence { get; set; }
}
