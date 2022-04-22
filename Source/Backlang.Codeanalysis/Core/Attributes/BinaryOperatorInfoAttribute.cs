using Backlang.Codeanalysis.Core.Attributes;
namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class BinaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public BinaryOperatorInfoAttribute(int precedence) : base(precedence, isUnary: false, isPostUnary: false)
    {
    }
}
