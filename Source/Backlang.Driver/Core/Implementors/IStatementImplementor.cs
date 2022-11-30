namespace Backlang.Driver.Core.Implementors;

public interface IStatementImplementor
{
    BasicBlockBuilder Implement(StatementParameters parameters);
}