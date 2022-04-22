namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class PreUnaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public PreUnaryOperatorInfoAttribute(int precedence) : base(precedence, isUnary: true, isPostUnary: false)
    {
    }
}
