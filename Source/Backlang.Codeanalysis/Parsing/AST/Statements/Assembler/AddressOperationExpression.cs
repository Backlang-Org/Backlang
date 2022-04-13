namespace Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

public sealed class AddressOperationExpression : Expression, IParsePoint<Expression>
{
    public Expression Expression { get; set; }

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        //[0xFF + 4]
        var node = new AddressOperationExpression();

        node.Expression = Expression.Parse(parser);

        iterator.Match(TokenType.CloseSquare);

        return node;
    }
}