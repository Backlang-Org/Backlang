namespace Backlang_Compiler.Core;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class PreUnaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public PreUnaryOperatorInfoAttribute(int precedence) : base(precedence, true, false)
    {
    }
}