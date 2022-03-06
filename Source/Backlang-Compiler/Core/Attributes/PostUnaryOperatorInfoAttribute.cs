namespace Backlang_Compiler.Core;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class PostUnaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public PostUnaryOperatorInfoAttribute(int precedence) : base(precedence, true, true)
    {
    }
}