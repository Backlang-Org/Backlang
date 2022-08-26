namespace Backlang.Core.CompilerService;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.ReturnValue)]
public class UnitAttribute : Attribute
{
    public Type UnitType { get; set; }
}