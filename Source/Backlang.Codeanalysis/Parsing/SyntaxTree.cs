using Backlang.Codeanalysis.Parsing.AST;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public static class SyntaxTree
{
    public static LNodeFactory Factory = new(EmptySourceFile.Unknown);

    public static LNode Annotation(LNode call)
    {
        return Factory.Call(Symbols.Annotation).PlusAttr(call);
    }

    public static LNode Constructor(LNodeList parameters, LNode code)
    {
        return Factory.Call(Symbols.Constructor, LNode.List(Factory.AltList(parameters), code));
    }

    public static LNode DiscriminatedType(Token nameToken, LNodeList parameters)
    {
        return Factory.Call(Symbols.DiscriminatedType,
            LNode.List(Factory.FromToken(nameToken), Factory.AltList(parameters)));
    }

    public static LNode DiscriminatedUnion(Token nameToken, LNodeList types)
    {
        return Factory.Call(Symbols.DiscriminatedUnion,
            LNode.List(Factory.FromToken(nameToken), Factory.AltList(types)));
    }

    public static LNode Property(LNode type, LNode name, LNode getter, LNode setter, LNode value)
    {
        if (value != null)
        {
            return LNode.Call(CodeSymbols.Property,
                LNode.List(type, getter, setter, LNode.Call(CodeSymbols.Assign, LNode.List(name, value))));
        }

        return LNode.Call(CodeSymbols.Property, LNode.List(type, getter, setter, name));
    }

    public static LNode Destructor(LNodeList parameters, LNode code)
    {
        return Factory.Call(Symbols.Destructor, LNode.List(Factory.AltList(parameters), code));
    }

    public static LNode Array(LNode typeNode, int dimensions)
    {
        return Factory.Call(CodeSymbols.Array, LNode.List(typeNode, LNode.Literal(dimensions)));
    }

    public static LNode Throw(LNode arg)
    {
        return Factory.Call(CodeSymbols.Throw, LNode.List(arg));
    }

    public static LNode ArrayInstantiation(LNodeList elements)
    {
        return Factory.Call(CodeSymbols.Braces, elements);
    }

    public static LNode ArrayInstantiation(LNode arr, LNodeList indices)
    {
        return arr.WithArgs(indices);
    }

    public static LNode Binary(Symbol op, LNode left, LNode right)
    {
        return Factory.Call(op, LNode.List(left, right)).SetStyle(NodeStyle.Operator);
    }

    public static LNode Bitfield(Token nameToken, LNodeList members)
    {
        return Factory.Call(Symbols.Bitfield, LNode.List(Factory.FromToken(nameToken), Factory.AltList(members)));
    }

    public static LNode Case(LNode condition, LNode body)
    {
        return Factory.Call(CodeSymbols.Case, LNode.List(condition, body));
    }

    public static LNode Catch(IdNode exceptionType, IdNode exceptionValueName, LNode body)
    {
        return Factory.Call(CodeSymbols.Catch, LNode.List(exceptionType, exceptionValueName, body));
    }

    public static LNode Class(Token nameToken, LNodeList inheritances, LNodeList members)
    {
        return Factory.Call(CodeSymbols.Class,
            LNode.List(
                Factory.FromToken(nameToken),
                Factory.Call(Symbols.Inheritance, inheritances),
                Factory.Call(CodeSymbols.Braces, members).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode Default()
    {
        return Default(LNode.Missing);
    }

    public static LNode Default(LNode type)
    {
        return Factory.Call(CodeSymbols.Default, LNode.List(type));
    }

    public static LNode Enum(LNode name, LNodeList members)
    {
        return Factory.Call(CodeSymbols.Enum, Factory.AltList(name,
            Factory.Call(CodeSymbols.AltList),
            Factory.Call(CodeSymbols.Braces,
                members)));
    }

    public static LNode For(LNode init, LNode arr, LNode body)
    {
        return Factory.Call(
            CodeSymbols.For,
            Factory.AltList(Factory.AltList(
                    Factory.AltList(LNode.Call(CodeSymbols.In,
                        Factory.AltList(init, arr)).SetStyle(NodeStyle.Operator))), LNode.Missing,
                Factory.AltList(), body));
    }

    public static LNode If(LNode cond, LNode ifBody, LNode elseBody)
    {
        return Factory.Call(CodeSymbols.If, Factory.AltList(cond, ifBody, elseBody));
    }

    public static LNode ImplDecl(LNode target, LNodeList body)
    {
        var attributes = new LNodeList();

        return Factory.Call(Symbols.Implementation,
            Factory.AltList(target, LNode.Call(CodeSymbols.Braces,
                body).SetStyle(NodeStyle.StatementBlock))).WithAttrs(attributes);
    }

    public static LNode Import(LNode expr)
    {
        return Factory.Call(CodeSymbols.Import, LNode.List(expr));
    }

    public static LNode Interface(Token nameToken, LNodeList inheritances, LNodeList members)
    {
        return Factory.Call(CodeSymbols.Interface,
            LNode.List(
                Factory.FromToken(nameToken),
                LNode.Call(Symbols.Inheritance, inheritances),
                LNode.Call(CodeSymbols.Braces, members).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode Module(LNode ns)
    {
        return Factory.Call(CodeSymbols.Namespace, LNode.List(ns));
    }

    public static LNode None()
    {
        return Factory.Call(CodeSymbols.Void, LNode.Literal(null));
    }

    public static LNode Pointer(LNode type)
    {
        return Factory.Call(Symbols.PointerType, LNode.List(type));
    }

    public static LNode RefType(LNode type)
    {
        return Factory.Call(Symbols.RefType, LNode.List(type));
    }

    public static LNode NullableType(LNode type)
    {
        return Factory.Call(Symbols.NullableType, LNode.List(type));
    }

    public static LNode Signature(LNode name, LNode type, LNodeList args, LNodeList generics)
    {
        return Factory.Call(CodeSymbols.Fn, LNode.List(
            type, name,
            Factory.AltList(args))).PlusAttr(LNode.Call(Symbols.Where, generics));
    }

    public static LNode SizeOf(LNode type)
    {
        return Factory.Call(CodeSymbols.Sizeof, LNode.List(type));
    }

    public static LNode Struct(Token nameToken, LNodeList inheritances, LNodeList members)
    {
        return Factory.Call(CodeSymbols.Struct,
            LNode.List(
                Factory.FromToken(nameToken),
                Factory.Call(Symbols.Inheritance, inheritances),
                Factory.Call(CodeSymbols.Braces, members).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode Switch(LNode element, LNodeList cases)
    {
        return Factory.Call(CodeSymbols.SwitchStmt,
            LNode.List(element, LNode.Call(CodeSymbols.Braces, cases).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode Try(LNode body, LNode catches, LNode finallly)
    {
        return Factory.Call(CodeSymbols.Try, LNode.List(
            body,
            catches,
            Factory.Call(CodeSymbols.Finally, LNode.List(finallly))));
    }

    public static LNode Type(string name, LNodeList arguments)
    {
        return Factory.Call(Symbols.TypeLiteral,
            Factory.AltList(Factory.Id(name), Factory.Call(CodeSymbols.Of, arguments)));
    }

    public static LNode Unary(Symbol op, LNode arg)
    {
        return Factory.Call(op, LNode.List(arg)).SetStyle(NodeStyle.Operator);
    }

    public static LNode Union(string name, LNodeList members)
    {
        return Factory.Call(Symbols.Union, LNode.List(Factory.Id(name)).Add(Factory.AltList(members)));
    }

    public static LNode Using(LNode expr)
    {
        if (!expr.Calls(CodeSymbols.As)) // TODO: throw error in intermediate stage when has only one arg
        {
            return Factory.Call(CodeSymbols.UsingStmt, LNode.Missing);
        }

        return Factory.Call(CodeSymbols.UsingStmt, LNode.List(expr[0], expr[1]));
    }

    public static LNode When(LNode binOp, LNode rightHand, LNode body)
    {
        return Factory.Call(CodeSymbols.When, LNode.List(binOp, rightHand, body));
    }

    public static LNode While(LNode cond, LNode body)
    {
        return Factory.Call(
            CodeSymbols.While,
            LNode.List(cond, body));
    }

    public static LNode Unit(LNode result, string unit)
    {
        return Factory.Call(Symbols.Unit, LNode.List(result, LNode.Id(unit)));
    }

    public static LNode UnitDeclaration(Token nameToken)
    {
        return Factory.Call(Symbols.UnitDecl, LNode.List(Factory.FromToken(nameToken)));
    }

    public static LNode TypeOfExpression(LNode type)
    {
        return Factory.Call(CodeSymbols.Typeof, LNode.List(type));
    }

    public static LNode DoWhile(LNode body, LNode cond)
    {
        return Factory.Call(CodeSymbols.DoWhile, LNode.List(body, cond));
    }
}