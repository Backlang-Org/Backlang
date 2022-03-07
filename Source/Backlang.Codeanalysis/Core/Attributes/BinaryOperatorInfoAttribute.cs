using Backlang.Codeanalysis.Core.Attributes;
namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class BinaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public BinaryOperatorInfoAttribute(int precedence) : base(precedence, false, false)
    {
    }
}
