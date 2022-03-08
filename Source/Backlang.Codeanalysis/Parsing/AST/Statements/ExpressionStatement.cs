﻿using Backlang.Codeanalysis.Parsing.AST.Statements;
namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class ExpressionStatement : Statement
{
    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
    }

    public Expression Expression { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}