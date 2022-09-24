namespace Backlang.ResourcePreprocessor.Mif.MifFormat.AST;

public class MifFile
{
    public Dictionary<string, object> Options { get; } = new();
    public List<MifDataRule> DataRules { get; } = new();
}