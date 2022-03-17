namespace Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

public class LabelReferenceExpression : Expression, IParsePoint<Expression>
{
    public string Label { get; set; }

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        var node = new LabelReferenceExpression();

        node.Label = iterator.Current.Text;

        iterator.NextToken();

        return node;
    }
}