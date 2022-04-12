using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
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

namespace Backlang_Compiler.Compiling.Stages;

public partial class IntermediateStage : IHandler<CompilerContext, CompilerContext>
{
    public IAssembly Assembly { get; private set; }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        var freeFunctions = new List<FunctionDeclaration>();

        context.Assembly = new DescribedAssembly(new QualifiedName("Example"));
        var type = new DescribedType(new QualifiedName("Program"), context.Assembly);

        foreach (var tree in context.Trees)
        {
            freeFunctions.AddRange(tree.Body.Body.OfType<FunctionDeclaration>());
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

            var method = new DescribedBodyMethod(type, new QualifiedName(function.Name.Text).FullyUnqualifiedName, function.IsStatic, new VoidType());

            method.Body = body;
            method.IsStatic = function.IsStatic;

            AddParameters(method, function);
            SetReturnType(method, function);

            type.AddMethod(method);
            context.Assembly.AddType(type);
        }

        return await next.Invoke(context);
    }

    private static Instruction ConvertExpression(IType elementType, Expression value)
    {
        if (value is LiteralNode lit)
        {
            return Instruction.CreateConstant(
                                           new IntegerConstant((int)lit.Value, IntegerSpec.UInt32),
                                           elementType);
        }

        return Instruction.CreateConstant(
                                           new IntegerConstant(1, IntegerSpec.UInt32),
                                           elementType);
    }

    private void AddParameters(DescribedMethod method, FunctionDeclaration function)
    {
        foreach (var p in function.Parameters)
        {
            method.AddParameter(new Parameter(GetType(p.Type.Typename), p.Name.Text));
        }
    }

    private MethodBody CompileBody(FunctionDeclaration function, CompilerContext context)
    {
        var graph = new FlowGraphBuilder();

        // Use a permissive exception delayability model to make the optimizer's
        // life easier.
        graph.AddAnalysis(
            new ConstantAnalysis<ExceptionDelayability>(
                PermissiveExceptionDelayability.Instance));

        // Grab the entry point block.
        var block = graph.EntryPoint;

        foreach (var node in function.Body.Body)
        {
            if (node is VariableDeclarationStatement variableDecl)
            {
                var elementType = GetType(variableDecl.Type?.Typename);
                var local = block.AppendInstruction(
                    Instruction.CreateAlloca(elementType));

                if (variableDecl.Value != null)
                {
                    block.AppendInstruction(
                       Instruction.CreateStore(
                           elementType,
                           local,
                           block.AppendInstruction(
                               ConvertExpression(elementType, variableDecl.Value))));
                }
            }
            else if (node is BinaryExpression variableAss && variableAss.OperatorToken.Type == TokenType.EqualsToken)
            {
            }
        }

        block.Flow = new ReturnFlow(
                Instruction.CreateConstant(DefaultConstant.Instance, new VoidType()));

        // Finish up the method body.
        return new MethodBody(
            new Parameter(),
            default(Parameter),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    private IType GetType(string name)
    {
        switch (name)
        {
            case "u32": return new U32Type();
            default:
                break;
        }

        return null;
    }

    private void SetReturnType(DescribedMethod method, FunctionDeclaration function)
    {
        if (function.ReturnType != null)
        {
            method.ReturnParameter = new Parameter(new VoidType());
        }
        else
        {
            method.ReturnParameter = new Parameter(GetType(function.ReturnType?.Typename));
        }
    }
}