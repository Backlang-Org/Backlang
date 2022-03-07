namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class PreUnaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public PreUnaryOperatorInfoAttribute(int precedence) : base(precedence, true, false)
    {
    }
}
