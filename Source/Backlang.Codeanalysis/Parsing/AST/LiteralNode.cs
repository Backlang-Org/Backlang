﻿namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class LiteralNode : Expression
{
    public LiteralNode(object value)
    {
        Value = value;
    }

    public object Value { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString()
    {
        return $"{Value}";
    }
}
