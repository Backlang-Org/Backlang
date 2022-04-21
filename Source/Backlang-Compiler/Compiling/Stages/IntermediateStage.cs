using Backlang_Compiler.Compiling.Typesystem;
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

namespace Backlang_Compiler.Compiling.Stages;

public sealed partial class IntermediateStage : IHandler<CompilerContext, CompilerContext>
{
    public static IType GetLiteralType(object value, TypeResolver resolver)
    {
        if (value is string) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(string));
        if (value is IdNode id) { } //todo: symbol table

        return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        var freeFunctions = new List<LNode>();

        context.Assembly = new DescribedAssembly(new QualifiedName("Example"));
        var type = new DescribedType(new SimpleName("Program").Qualify("Example"), context.Assembly);

        foreach (var tree in context.Trees)
        {
            freeFunctions.AddRange(tree.Body.Where(_ => _.IsCall && _.Name == CodeSymbols.Fn));
        }

        foreach (var function in freeFunctions)
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

            var method = new DescribedBodyMethod(type,
                new QualifiedName(function.Args[1].Name.Name).FullyUnqualifiedName,
                function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)))
            {
                Body = body
            };

            AddParameters(method, function, context.Binder);
            SetReturnType(method, function, context.Binder);

            type.AddMethod(method);
            context.Assembly.AddType(type);
        }

        return await next.Invoke(context).ConfigureAwait(false);
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

    private static MethodBody CompileBody(LNode function, CompilerContext context)
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

    private static Parameter ConvertParameter(LNode p, TypeResolver resolver)
    {
        var type = GetType(p.Args[0], resolver);
        var assignment = p.Args[1];

        var name = assignment.Args[0].Name;

        return new Parameter(type, name.ToString());
    }

    private static IType GetType(LNode type, TypeResolver resolver)
    {
        var name = type.Args[0].Name.ToString();
        switch (name)
        {
            case "u32": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(uint));
            case "string": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(string));
            case "void": return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
            default:
                return ClrTypeEnvironmentBuilder.ResolveType(resolver, name, "Example");
        }

        return null;
    }

    private static void SetReturnType(DescribedBodyMethod method, LNode function, TypeResolver resolver)
    {
        var retType = function.Args[0];
        method.ReturnParameter = new Parameter(GetType(retType, resolver));
    }
}