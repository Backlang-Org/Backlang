using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Driver.Core;
using Backlang.Driver.Core.Implementors;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Loyc;
using Loyc.Syntax;
using System.Collections.Immutable;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public partial class ImplementationStage
{
    private static readonly ImmutableDictionary<Symbol, IImplementor> _implementations = new Dictionary<Symbol, IImplementor>()
    {
        [CodeSymbols.Var] = new VariableImplementor(),
        [CodeSymbols.If] = new IfImplementor()
    }.ToImmutableDictionary();

    public static MethodBody CompileBody(LNode function, CompilerContext context, IMethod method,
                QualifiedName? modulename, Scope scope)
    {
        var graph = Utils.CreateGraphBuilder();
        var block = graph.EntryPoint;

        AppendBlock(function.Args[3], block, context, method, modulename, scope);

        return new MethodBody(
            method.ReturnParameter,
            new Parameter(method.ParentType),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    private static void ConvertMethodBodies(CompilerContext context)
    {
        foreach (var bodyCompilation in context.BodyCompilations)
        {
            bodyCompilation.method.Body =
                CompileBody(bodyCompilation.function, context,
                bodyCompilation.method, bodyCompilation.modulename, bodyCompilation.scope);
        }
    }

    public static BasicBlockBuilder AppendBlock(LNode blkNode, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope)
    {
        foreach (var node in blkNode.Args)
        {
            if (!node.IsCall) continue;

            if (node.Calls(Symbols.Block))
            {
                if (node.ArgCount == 0) continue;

                block = AppendBlock(node, block.Graph.AddBasicBlock(), context, method, modulename, scope.CreateChildScope());
            }

            if (_implementations.ContainsKey(node.Name))
            {
                block = _implementations[node.Name].Implement(context, method, block, node, modulename, scope);
                continue;
            }
            
            if (node.Calls(CodeSymbols.While))
            {
                block = AppendWhile(context, method, block, node, modulename, scope);
            }
            else if (node.Calls("print"))
            {
                AppendCall(context, block, node, context.writeMethods, "Write");
            }
            else if (node.Calls(CodeSymbols.Return))
            {
                if (node.ArgCount == 1)
                {
                    var valueNode = node.Args[0];

                    AppendExpression(block, valueNode, (DescribedType)context.Environment.Int32, method); //ToDo: Deduce Type

                    block.Flow = new ReturnFlow();
                }
                else
                {
                    block.Flow = new ReturnFlow();
                }
            }
            else if (node.Calls(CodeSymbols.Throw))
            {
                var valueNode = node.Args[0].Args[0];
                var constant = block.AppendInstruction(ConvertConstant(
                    GetLiteralType(valueNode, context.Binder), valueNode.Value));

                var msg = block.AppendInstruction(Instruction.CreateLoad(GetLiteralType(valueNode, context.Binder), constant));

                if (node.Args[0].Name.Name == "#string")
                {
                    var exceptionType = Utils.ResolveType(context.Binder, typeof(Exception));
                    var exceptionCtor = exceptionType.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == 1);

                    block.AppendInstruction(Instruction.CreateNewObject(exceptionCtor, new List<ValueTag> { msg }));
                }

                block.Flow = UnreachableFlow.Instance;
            }
            else if (node.Calls(Symbols.ColonColon))
            {
                var callee = node.Args[1];
                var typename = ConversionUtils.GetQualifiedName(node.Args[0]);

                var type = (DescribedType)context.Binder.ResolveTypes(typename).FirstOrDefault();

                AppendCall(context, block, callee, type.Methods, callee.Name.Name);
            }
            else if (node.Calls(CodeSymbols.Dot))
            {
                if (node is ("'.", var target, var callee) && target is ("this", _))
                {
                    //AppendThis(block, method.ParentType); // we do that already in AppendCall

                    var type = method.ParentType;
                    var call = type.Methods.FirstOrDefault(_ => _.Name.ToString() == callee.Name.Name);

                    if (callee != null)
                    {
                        AppendCall(context, block, callee, type.Methods);
                    }
                    else
                    {
                        context.AddError(node, $"Cannot find function '{callee.Name.Name}'");
                    }
                }
                else
                {
                    // ToDo: other things and so on...
                }
            }
            else
            {
                //ToDo: continue implementing static function call in same type
                var type = method.ParentType;
                var calleeName = node.Target;
                var callee = type.Methods.FirstOrDefault(_ => _.Name.ToString() == calleeName.Name.Name);

                if (callee != null)
                {
                    AppendCall(context, block, node, type.Methods);
                }
                else
                {
                    context.AddError(node, $"Cannot find function '{calleeName.Name.Name}'");
                }
            }
        }

        return block;
    }

    public static NamedInstructionBuilder AppendExpression(BasicBlockBuilder block, LNode node, IType elementType, IMethod method)
    {
        if (node.ArgCount == 1 && node.Args[0].HasValue)
        {
            var constant = ConvertConstant(elementType, node.Args[0].Value);
            var value = block.AppendInstruction(constant);

            return block.AppendInstruction(Instruction.CreateLoad(elementType, value));
        }
        else if (node.ArgCount == 2)
        {
            var lhs = AppendExpression(block, node.Args[0], elementType, method);
            var rhs = AppendExpression(block, node.Args[1], elementType, method);

            return block.AppendInstruction(Instruction.CreateBinaryArithmeticIntrinsic(node.Name.Name.Substring(1), false, elementType, lhs, rhs));
        }
        else if (node.IsId)
        {
            var par = method.Parameters.Where(_ => _.Name.ToString() == node.Name.Name);

            if (!par.Any())
            {
                var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == node.Name.Name);
                if (localPrms.Any())
                {
                    return block.AppendInstruction(Instruction.CreateLoadLocal(new Parameter(localPrms.First().Type, localPrms.First().Tag.Name)));
                }
            }
            else
            {
                return block.AppendInstruction(Instruction.CreateLoadArg(par.First()));
            }
        }
        else if (node is ("'&", var p))
        {
            var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == p.Name.Name);
            if (localPrms.Any())
            {
                return block.AppendInstruction(Instruction.CreateLoadLocalAdress(new Parameter(localPrms.First().Type, localPrms.First().Tag.Name)));
            }
        }
        else if (node is ("'*", var o) && node.ArgCount == 1)
        {
            var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == o.Name.Name);
            if (localPrms.Any())
            {
                AppendExpression(block, o, elementType, method);
                return block.AppendInstruction(Instruction.CreateLoadIndirect(localPrms.First().Type));
            }
        }

        return null;
    }

    private static BasicBlockBuilder AppendWhile(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, var condition, var body))
        {
            var while_start = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_start"));
            AppendBlock(body, while_start, context, method, modulename, scope.CreateChildScope());

            var while_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_condition"));
            AppendExpression(while_condition, condition, context.Environment.Boolean, method);
            while_condition.Flow = new JumpFlow(while_start);

            var while_end = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_end"));
            block.Flow = new JumpFlow(while_condition);

            while_start.Flow = new JumpFlow(while_end);

            if (condition.Calls(CodeSymbols.Bool))
            {
                while_end.Flow = new JumpConditionalFlow(while_start, ConditionalJumpKind.True);
            }
            else
            {
                AppendExpression(block, condition, method.ParentType, method);
                while_end.Flow = new JumpConditionalFlow(while_start, ConditionalJumpKind.Equals);
            }

            return block.Graph.AddBasicBlock();
        }

        return null;
    }

    private static void AppendCall(CompilerContext context, BasicBlockBuilder block,
        LNode node, IEnumerable<IMethod> methods, string methodName = null)
    {
        var argTypes = new List<IType>();
        var callTags = new List<ValueTag>();

        foreach (var arg in node.Args)
        {
            var type = GetLiteralType(arg, context.Binder);
            argTypes.Add(type);

            var constant = block.AppendInstruction(
            ConvertConstant(type, arg.Args[0].Value));

            block.AppendInstruction(Instruction.CreateLoad(type, constant));

            callTags.Add(constant);
        }

        if (methodName == null)
        {
            methodName = node.Name.Name;
        }

        var method = GetMatchingMethod(context, argTypes, methods, methodName);

        if (method == null) return;

        if (!method.IsStatic)
        {
            callTags.Insert(0, block.AppendInstruction(Instruction.CreateLoadArg(new Parameter(method.ParentType))));
        }

        var call = Instruction.CreateCall(method, method.IsStatic ? MethodLookup.Static : MethodLookup.Virtual, callTags);

        block.AppendInstruction(call);
    }
}