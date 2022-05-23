﻿using Backlang.Codeanalysis.Parsing.AST;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public static class SyntaxTree
{
    public static LNodeFactory Factory = new(EmptySourceFile.Unknown);

    public static LNode Array(LNode typeNode, int dimensions)
    {
        return LNode.Call(CodeSymbols.Array, LNode.List(typeNode, LNode.Literal(dimensions)));
    }

    public static LNode ArrayInstantiation(LNodeList elements)
    {
        return LNode.Call(CodeSymbols.Braces, elements);
    }

    public static LNode ArrayInstantiation(IdNode arr, LNodeList indices)
    {
        return arr.WithArgs(indices);
    }

    public static LNode Binary(Symbol op, LNode left, LNode right)
    {
        return LNode.Call(op, LNode.List(left, right)).SetStyle(NodeStyle.Operator);
    }

    public static LNode Bitfield(string name, LNodeList members)
    {
        return LNode.Call(Symbols.Bitfield, LNode.Id(name)).WithAttrs(members);
    }

    public static LNode Default()
    {
        return Default(LNode.Missing);
    }

    public static LNode Default(LNode type)
    {
        return LNode.Call(CodeSymbols.Default, LNode.List(type));
    }

    public static LNode Enum(LNode name, LNodeList members)
    {
        return LNode.Call(CodeSymbols.Enum, LNode.List(name,
            LNode.Call(CodeSymbols.AltList),
              LNode.Call(CodeSymbols.Braces,
                  members)));
    }

    public static LNode Fn(LNode name, LNode type, LNodeList args, LNodeList body)
    {
        return LNode.Call(CodeSymbols.Fn, LNode.List(type, name,
                LNode.Call(CodeSymbols.AltList, args), LNode.Call(CodeSymbols.Braces, body).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode For(LNode init, LNode arr, LNodeList body)
    {
        return LNode.Call(
            CodeSymbols.For,
                LNode.List(LNode.Call(CodeSymbols.AltList,
                    LNode.List(LNode.Call(CodeSymbols.In,
                        LNode.List(init, arr)).SetStyle(NodeStyle.Operator))), LNode.Missing,
                            LNode.Call(CodeSymbols.AltList),
                                LNode.Call(CodeSymbols.Braces,
                                    body).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode If(LNode cond, LNodeList ifBody, LNodeList elseBody)
    {
        return LNode.Call(CodeSymbols.If,
            LNode.List(cond, LNode.Call(CodeSymbols.Braces,
                ifBody).SetStyle(NodeStyle.StatementBlock),
                    LNode.Call(CodeSymbols.Braces,
                        elseBody).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode ImplDecl(LNode target, LNodeList body, bool isStatic)
    {
        var attributes = new LNodeList();

        if (isStatic)
        {
            attributes.Add(LNode.Call(CodeSymbols.Static));
        }

        return LNode.Call(attributes, Symbols.Implementation,
           LNode.List(target, LNode.Call(CodeSymbols.Braces,
               body).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode Import(LNode expr)
    {
        return LNode.Call(CodeSymbols.Import, LNode.List(expr));
    }

    public static LNode Module(LNode ns)
    {
        return LNode.Call(CodeSymbols.Namespace, LNode.List(ns));
    }

    public static LNode None()
    {
        return LNode.Call(CodeSymbols.Void);
    }

    public static LNode Pointer(LNode type)
    {
        return LNode.Call(Symbols.PointerType, LNode.List(type));
    }

    public static LNode SizeOf(LNode type)
    {
        return LNode.Call(CodeSymbols.Sizeof, LNode.List(type));
    }

    public static LNode Struct(string name, LNodeList members)
    {
        return LNode.Call(CodeSymbols.Struct,
            LNode.List(LNode.Id((Symbol)name),
                LNode.Call(CodeSymbols.AltList),
                    LNode.Call(CodeSymbols.Braces,
                       members).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode Type(string name, LNodeList arguments)
    {
        return LNode.Call(Symbols.TypeLiteral, LNode.List(LNode.Id(name), LNode.Call(CodeSymbols.Of, arguments)));
    }

    public static LNode Unary(Symbol op, LNode arg)
    {
        return LNode.Call(op, LNode.List(arg)).SetStyle(NodeStyle.Operator);
    }

    public static LNode While(LNode cond, LNodeList body)
    {
        return LNode.Call(
            CodeSymbols.While,
                LNode.List(cond,
                    LNode.Call(CodeSymbols.Braces,
                        body).SetStyle(NodeStyle.StatementBlock)));
    }
}