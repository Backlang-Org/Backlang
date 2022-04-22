using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.Transforms;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc;
using Loyc.Syntax;
using System.Globalization;

namespace Backlang.Driver.Compiling.Stages;

public sealed class IntermediateStage : IHandler<CompilerContext, CompilerContext>
{
    public static MethodBody CompileBody(LNode function, CompilerContext context)
    {
        var graph = new FlowGraphBuilder();

        // Use a permissive exception delayability model to make the optimizer's
        // life easier.
        graph.AddAnalysis(
            new ConstantAnalysis<ExceptionDelayability>(
                PermissiveExceptionDelayability.Instance));

        // Grab the entry point block.
        var block = graph.EntryPoint;

        foreach (var node in function.Args[3].Args)
        {
            if (!node.IsCall) continue;

            if (node.Name == CodeSymbols.Var)
            {
                var elementType = GetType(node.Args[0], context.Binder);

                var local = block.AppendInstruction(
                     Instruction.CreateAlloca(elementType));

                var decl = node.Args[1];
                if (decl.Args[1].HasValue)
                {
                    block.AppendInstruction(
                       Instruction.CreateStore(
                           elementType,
                           local,
                           block.AppendInstruction(
                               ConvertExpression(elementType, decl.Args[1].Value))));
                }
            }
            else if (node.Name == (Symbol)"print")
            {
                var method = context.writeMethods.FirstOrDefault();
                var constant = block.AppendInstruction(
                    ConvertExpression(
                        GetLiteralType(node.Args[0].Value, context.Binder),
                    node.Args[0].Value.ToString()));

                var str = block.AppendInstruction(
                    Instruction.CreateLoad(
                        GetLiteralType(node.Args[0].Value, context.Binder), constant));

                block.AppendInstruction(Instruction.CreateCall(method, MethodLookup.Static, new ValueTag[] { str }));
            }
        }

        block.Flow = new ReturnFlow(
                Instruction.CreateConstant(DefaultConstant.Instance, ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void))));

        // Finish up the method body.
        return new MethodBody(
            new Parameter(),
            default,
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    public static DescribedBodyMethod ConvertFunction(CompilerContext context, DescribedType type, LNode function)
    {
        var sourceBody = CompileBody(function, context);

        var body = sourceBody.WithImplementation(
            sourceBody.Implementation.Transform(
                AllocaToRegister.Instance,
                CopyPropagation.Instance,
                new ConstantPropagation(),
                GlobalValueNumbering.Instance,
                CopyPropagation.Instance,
                DeadValueElimination.Instance,
                MemoryAccessElimination.Instance,
                CopyPropagation.Instance,
                new ConstantPropagation(),
                DeadValueElimination.Instance,
                ReassociateOperators.Instance,
                DeadValueElimination.Instance));

        string methodName = function.Args[1].Name.Name;
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Operator)))
        {
            function.Attrs.Add(LNode.Id(CodeSymbols.Static));
            methodName = ConvertMethodNameToOperator(methodName);
        }

        var method = new DescribedBodyMethod(type,
            new QualifiedName(methodName).FullyUnqualifiedName,
            function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)))
        {
            Body = body
        };

        var modifier = AccessModifierAttribute.Create(AccessModifier.Public);
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Private)))
        {
            modifier = AccessModifierAttribute.Create(AccessModifier.Private);
        }

        method.AddAttribute(modifier);

        AddParameters(method, function, context.Binder);
        SetReturnType(method, function, context.Binder);

        return method;
    }

    public static IType GetLiteralType(object value, TypeResolver resolver)
    {
        if (value is string) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(string));
        if (value is IdNode id) { } //todo: symbol table

        return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
    }

    public static IType GetType(LNode type, TypeResolver resolver)
    {
        var name = type.Args[0].Name.ToString();
        switch (name)
        {
            case "u32": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(uint));
            case "u8": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(byte));
            case "u16": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(ushort));
            case "string": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(string));
            case "void": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
            default:
                return ClrTypeEnvironmentBuilder.ResolveType(resolver, name, "Example");
        }

        return null;
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Assembly = new DescribedAssembly(new QualifiedName("Example"));

        foreach (var tree in context.Trees)
        {
            ConvertFreeFunctions(context, tree);

            ConvertStructs(context, tree);
        }

        context.Assembly.AddType(context.ExtensionsType);
        context.Binder.AddAssembly(context.Assembly);

        return await next.Invoke(context);
    }

    private static void AddParameters(DescribedBodyMethod method, LNode function, TypeResolver resolver)
    {
        var param = function.Args[2];

        foreach (var p in param.Args)
        {
            var pa = ConvertParameter(p, resolver);
            method.AddParameter(pa);
        }
    }

    private static Instruction ConvertExpression(IType elementType, object value)
    {
        if (value is int i)
        {
            return Instruction.CreateConstant(
                                           new IntegerConstant(i, IntegerSpec.UInt32),
                                           elementType);
        }
        else if (value is string str)
        {
            return Instruction.CreateConstant(
                                           new StringConstant(str),
                                           elementType);
        }

        return Instruction.CreateConstant(
                                           new IntegerConstant(0, IntegerSpec.UInt32),
                                           elementType);
    }

    private static void ConvertFreeFunctions(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
    {
        var ff = tree.Body.Where(_ => _.IsCall && _.Name == CodeSymbols.Fn);

        foreach (var function in ff)
        {
            DescribedType type;

            if (!context.Assembly.Types.Any(_ => _.FullName.FullName == "Example.Program"))
            {
                type = new DescribedType(new SimpleName("Program").Qualify("Example"), context.Assembly);
                context.Assembly.AddType(type);
            }
            else
            {
                type = (DescribedType)context.Assembly.Types.First(_ => _.FullName.FullName == "Example.Program");
            }

            var method = ConvertFunction(context, type, function);

            type.AddMethod(method);
        }
    }

    private static string ConvertMethodNameToOperator(string methodName)
    {
        TextInfo info = CultureInfo.InvariantCulture.TextInfo;
        var m = info.ToTitleCase(methodName); //ToDo: convert to opmap: greaterThen -> GreaterThan

        return $"op_{m}";
    }

    private static Parameter ConvertParameter(LNode p, TypeResolver resolver)
    {
        var type = GetType(p.Args[0], resolver);
        var assignment = p.Args[1];

        var name = assignment.Args[0].Name;

        return new Parameter(type, name.ToString());
    }

    private static void ConvertStructMembers(LNode members, DescribedType type, CompilerContext context)
    {
        foreach (var member in members.Args)
        {
            if (member.Name == CodeSymbols.Var)
            {
                var mtype = GetType(member.Args[0], context.Binder);

                var mvar = member.Args[1];
                var mname = mvar.Args[0].Name;

                var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

                type.AddField(field);
            }
        }
    }

    private static void ConvertStructs(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
    {
        var structs = tree.Body.Where(_ => _.IsCall && _.Name == CodeSymbols.Struct);

        foreach (var st in structs)
        {
            var name = st.Args[0].Name;
            var members = st.Args[2];

            var type = new DescribedType(new SimpleName(name.Name).Qualify("Example"), context.Assembly);
            type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("ValueType").Qualify("System")).First());

            type.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

            ConvertStructMembers(members, type, context);

            context.Assembly.AddType(type);
        }
    }

    private static void SetReturnType(DescribedBodyMethod method, LNode function, TypeResolver resolver)
    {
        var retType = function.Args[0];
        method.ReturnParameter = new Parameter(GetType(retType, resolver));
    }
}