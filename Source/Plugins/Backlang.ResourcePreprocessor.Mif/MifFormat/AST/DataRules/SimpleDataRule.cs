namespace Backlang.ResourcePreprocessor.Mif.MifFormat.AST.DataRules;

public class SimpleDataRule : MifDataRule
{
    public SimpleDataRule(int addr, long value)
    {
        Address = addr;
        Value = value;
    }

    public int Address { get; set; }
    public long Value { get; set; }
}