namespace Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

public class AssemblerBlockStatement : Statement, IParsePoint<Statement>
{
    public static readonly ParsePoints<Expression> ExpressionParsePoints = new();

    static AssemblerBlockStatement()
    {
        AddAssemblerExpressionParsePoint<AddressOperationExpression>(TokenType.OpenSquare);
        AddAssemblerExpressionParsePoint<LabelReferenceExpression>(TokenType.Dollar);
        AddAssemblerExpressionParsePoint<RegisterReferenceExpression>(TokenType.Identifier);
    }

    public List<AssemblerBodyNode> Body { get; set; } = new();

    public static void AddAssemblerExpressionParsePoint<T>(TokenType type)
            where T : IParsePoint<Expression>
    {
        ExpressionParsePoints.Add(type, T.Parse);
    }

    public static Statement Parse(TokenIterator iterator, Parser parser)
    {
        var node = new AssemblerBlockStatement();

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            if (iterator.Peek(1).Type == TokenType.OpenCurly)
            {
                // label block:
                var labelBlock = new LabelBlockDefinition();
                labelBlock.Name = iterator.NextToken().Text;
                labelBlock.Body = ((AssemblerBlockStatement)Parse(iterator, parser)).Body;

                node.Body.Add(labelBlock);
            }
            else
            {
                // instruction
                // opcode arg1, arg2, ... ;
                var instruction = new Instruction();

                //ToDo: Check is opcode name is valid: Opcode Enum TryParse
                instruction.OpCode = iterator.NextToken().Text;

                while (iterator.Current.Type != TokenType.Semicolon)
                {
                    instruction.Arguments.Add(Expression.Parse(parser, ExpressionParsePoints));

                    if (iterator.Current.Type == TokenType.Semicolon)
                    {
                        break;
                    }
                    else
                    {
                        iterator.Match(TokenType.Comma);
                    }
                }

                iterator.Match(TokenType.Semicolon);

                node.Body.Add(instruction);
            }
        }

        iterator.Match(TokenType.CloseCurly);

        return node;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}