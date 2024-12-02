using Backlang.Codeanalysis.Parsing.Precedences;

namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class BinaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public BinaryOperatorInfoAttribute(int precedence) : base(precedence, false, false)
    {
    }

    public BinaryOperatorInfoAttribute(BinaryOpPrecedences precedence) : this((int)precedence)
    {
    }
}