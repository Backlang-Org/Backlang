namespace Backlang_Compiler.Core;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class BinaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public BinaryOperatorInfoAttribute(int precedence) : base(precedence, false, false)
    {
    }
}
