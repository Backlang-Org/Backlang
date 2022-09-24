namespace Backlang.ResourcePreprocessor.Mif.MifFormat.AST.DataRules;

public class RangeDataRule : MifDataRule
{
    public RangeDataRule(int from, int to, long value)
    {
        From = from;
        To = to;
        Value = value;
    }

    public int From { get; }
    public int To { get; }
    public long Value { get; }
}