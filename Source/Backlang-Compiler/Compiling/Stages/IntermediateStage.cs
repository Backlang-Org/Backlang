using Backlang_Compiler.Compiling.Typesystem.Types;
using Flo;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Transforms;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;

namespace Backlang_Compiler.Compiling.Stages;

public sealed partial class IntermediateStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        var freeFunctions = new List<LNode>();

        context.Assembly = new DescribedAssembly(new QualifiedName("Example"));
        var type = new DescribedType(new QualifiedName("Program"), context.Assembly);

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

            var method = new DescribedBodyMethod(type, new QualifiedName(function.Name.Name).FullyUnqualifiedName, function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), new VoidType())
            {
                Body = body
            };

            AddParameters(method, function);
            SetReturnType(method, function);

            type.AddMethod(method);
            context.Assembly.AddType(type);
        }

        return await next.Invoke(context).ConfigureAwait(false);
    }

    private static void AddParameters(DescribedBodyMethod method, LNode function)
    {
        var param = function.Args[2];

        foreach (var p in param.Args)
        {
            var pa = ConvertParameter(p);
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
            if (node.IsCall && node.Name == CodeSymbols.Var)
            {
                var elementType = GetType(node.Args[0]);

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
        }

        block.Flow = new ReturnFlow(
                Instruction.CreateConstant(DefaultConstant.Instance, new VoidType()));

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

    private static Parameter ConvertParameter(LNode p)
    {
        var type = GetType(p.Args[0]);
        var assignment = p.Args[1];

        var name = assignment.Args[0].Name;

        return new Parameter(type, name.ToString());
    }

    private static IType GetType(LNode type)
    {
        var name = type.Args[0].Name.ToString();
        switch (name)
        {
            case "u32": return new U32Type();
            case "string": return new StringType();
            case "void": return new VoidType();
            default:
                break;
        }

        return null;
    }

    private static void SetReturnType(DescribedBodyMethod method, LNode function)
    {
        var retType = function.Args[0];
        method.ReturnParameter = new Parameter(GetType(retType));
    }
}