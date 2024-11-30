using Backlang.Codeanalysis.Parsing.Precedences;

namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class PostUnaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public PostUnaryOperatorInfoAttribute(int precedence) : base(precedence, true, true)
    {
    }

    public PostUnaryOperatorInfoAttribute(UnaryOpPrecedences precedence) : this((int)precedence)
    {
    }
}