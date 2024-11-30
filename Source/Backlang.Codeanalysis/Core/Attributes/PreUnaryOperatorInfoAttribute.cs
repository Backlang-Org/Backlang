using Backlang.Codeanalysis.Parsing.Precedences;

namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class PreUnaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public PreUnaryOperatorInfoAttribute(int precedence) : base(precedence, true, false)
    {
    }

    public PreUnaryOperatorInfoAttribute(UnaryOpPrecedences precedence) : this((int)precedence)
    {
    }
}