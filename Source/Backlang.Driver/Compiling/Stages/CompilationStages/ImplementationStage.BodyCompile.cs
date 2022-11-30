using Backlang.Codeanalysis.Core;
using Backlang.Contracts.Scoping.Items;
using Backlang.Driver.Core.Implementors;
using Backlang.Driver.Core.Implementors.Expressions;
using Backlang.Driver.Core.Implementors.Statements;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public partial class ImplementationStage
{
    private static readonly ImmutableDictionary<Symbol, IStatementImplementor> _implementations = new Dictionary<Symbol, IStatementImplementor>()
    {
        [CodeSymbols.Var] = new VariableImplementor(),
        [CodeSymbols.Assign] = new AssignmentImplementor(),
        [CodeSymbols.If] = new IfImplementor(),
        [CodeSymbols.While] = new WhileImplementor(),
        [CodeSymbols.DoWhile] = new DoWhileImplementor(),
        [CodeSymbols.Return] = new ReturnImplementor(),
        [CodeSymbols.Throw] = new ThrowImplementor(),
        [CodeSymbols.ColonColon] = new StaticCallImplementor(),
        [CodeSymbols.Dot] = new CallImplementor(),
        [(Symbol)"print"] = new PrintOrPrintlnImplementor(),
        [(Symbol)"println"] = new PrintOrPrintlnImplementor(),
    }.ToImmutableDictionary();

    private static readonly ImmutableList<IExpressionImplementor> _expressions = new List<IExpressionImplementor>()
    {
        new TupleExpressionImplementor(),
        new ArrayExpressionImplementor(),
        new DefaultExpressionImplementor(),
        new TypeOfExpressionImplementor(),
        new AsExpressionImplementor(),
        new AddressExpressionImplementor(),
        new CtorExpressionImplementor(),
        new UnaryExpressionImplementor(),
        new MemberExpressionImplementor(),
        new BinaryExpressionImplementor(),
        new IdentifierExpressionImplementor(),
        new PointerExpressionImplementor(),
        new ConstantExpressionEmitter(),
        new StaticCallImplementor(),

        new CallExpressionEmitter(), //should be added as last
    }.ToImmutableList();

    private static void SetReturnType(DescribedBodyMethod method, LNode function, CompilerContext context, Scope scope, QualifiedName modulename)
    {
        var retType = function.Args[0];

        if (retType.Name != LNode.Missing.Name)
        {
            var rtype = TypeInheritanceStage.ResolveTypeWithModule(retType, context, modulename);

            method.ReturnParameter = new Parameter(rtype);
        }
        else
        {
            var deducedReturnType = TypeDeducer.DeduceFunctionReturnType(function, context, scope, modulename);

            method.ReturnParameter = deducedReturnType != null ? new Parameter(deducedReturnType) : new Parameter(Utils.ResolveType(context.Binder, typeof(void)));
        }
    }
    public static MethodBody CompileBody(LNode function, CompilerContext context, IMethod method,
                    QualifiedName? modulename, Scope scope)
    {
        var graph = Utils.CreateGraphBuilder();
        var block = graph.EntryPoint;

        var afterBlock  = AppendBlock(function.Args[3], block, context, method, modulename, scope);

        if(afterBlock.Flow is NothingFlow)
        {
            afterBlock.Flow = new ReturnFlow();
        }

        SetReturnType((DescribedBodyMethod)method, function, context, scope, modulename.Value);

        return new MethodBody(
            method.ReturnParameter,
            method.IsStatic ? new Parameter() : Parameter.CreateThisParameter(method.ParentType),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    public static BasicBlockBuilder AppendBlock(LNode blkNode, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope)
    {
        block.Flow = new NothingFlow();

        foreach (var node in blkNode.Args)
        {
            if (!node.IsCall) continue;

            if (node.Calls(CodeSymbols.Braces))
            {
                if (node.ArgCount == 0) continue;

                block = AppendBlock(node, block.Graph.AddBasicBlock(), context, method, modulename, scope.CreateChildScope());
                continue;
            }

            if (_implementations.ContainsKey(node.Name))
            {
                block = _implementations[node.Name].Implement(context, method, block, node, modulename, scope);
            }
            else
            {
                EmitFunctionCall(method, node, block, context, scope, modulename);
            }
        }

        //automatic dtor call
        AppendAllDtors(block, context, modulename, scope);

        return block;
    }

    public static NamedInstructionBuilder AppendExpression(BasicBlockBuilder block, LNode node,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var fetch = _expressions.FirstOrDefault(_ => _.CanHandle(node));

        return fetch == null ? null : fetch.Handle(node, block, elementType, context, scope, modulename);
    }

    public static NamedInstructionBuilder AppendCall(CompilerContext context, BasicBlockBuilder block,
        LNode node, IEnumerable<IMethod> methods, Scope scope, QualifiedName? modulename, bool shouldAppendError = true, string methodName = null)
    {
        var argTypes = new List<IType>();

        foreach (var arg in node.Args)
        {
            var type = TypeDeducer.Deduce(arg, scope, context, modulename.Value);

            if (type != null)
                argTypes.Add(type);
        }

        if (methodName == null)
        {
            methodName = node.Name.Name;
        }

        var method = GetMatchingMethod(context, argTypes, methods, methodName, shouldAppendError);

        return AppendCall(context, block, node, method, scope, modulename.Value);
    }

    public static NamedInstructionBuilder AppendDtor(CompilerContext context, BasicBlockBuilder block, Scope scope, QualifiedName? modulename, string varname)
    {
        if (scope.TryGet<VariableScopeItem>(varname, out var scopeItem))
        {
            if (!scopeItem.Type.Methods.Any(_ => _.Name.ToString() == "Finalize"))
            {
                return null;
            }

            block.AppendInstruction(Instruction.CreateLoadLocal(scopeItem.Parameter));

            return AppendCall(context, block, LNode.Missing, scopeItem.Type.Methods, scope, modulename, false, "Finalize");
        }

        return null;
    }

    public static NamedInstructionBuilder AppendCall(CompilerContext context, BasicBlockBuilder block,
        LNode node, IMethod method, Scope scope, QualifiedName? modulename)
    {
        var callTags = AppendCallArguments(context, block, node, scope, modulename);

        if (method == null) return null;

        if (!method.IsStatic)
        {
            callTags.Insert(0, block.AppendInstruction(Instruction.CreateLoadArg(new Parameter(method.ParentType))));
        }

        var call = Instruction.CreateCall(method, method.IsStatic ? MethodLookup.Static : MethodLookup.Virtual, callTags);

        return block.AppendInstruction(call);
    }

    public static List<ValueTag> AppendCallArguments(CompilerContext context, BasicBlockBuilder block, LNode node, Scope scope, QualifiedName? modulename)
    {
        var argTypes = new List<IType>();
        var callTags = new List<ValueTag>();

        foreach (var arg in node.Args)
        {
            var type = TypeDeducer.Deduce(arg, scope, context, modulename.Value);
            argTypes.Add(type);

            if (!arg.IsId)
            {
                var val = AppendExpression(block, arg, type, context, scope, modulename);

                callTags.Add(val);
            }
            else
            {
                if (scope.TryGet<ScopeItem>(arg.Name.Name, out var scopeItem))
                {
                    ValueTag vt = null;
                    if (scopeItem is VariableScopeItem vsi)
                    {
                        vt = block.AppendInstruction(Instruction.CreateLoadLocal(vsi.Parameter));
                    }
                    else if (scopeItem is ParameterScopeItem psi)
                    {
                        vt = block.AppendInstruction(Instruction.CreateLoadArg(psi.Parameter));
                    }
                    else if (scopeItem is FieldScopeItem fsi)
                    {
                        vt = block.AppendInstruction(Instruction.CreateLoadField(fsi.Field));
                    }

                    callTags.Add(vt);
                }
                else
                {
                    var suggestion = LevensteinDistance.Suggest(arg.Name.Name, scope.GetAllScopeNames());

                    context.AddError(arg, new(ErrorID.CannotBeFoundDidYouMean, arg.Name.Name, suggestion));
                }
            }
        }

        return callTags;
    }

    public static void AppendAllDtors(BasicBlockBuilder block, CompilerContext context, QualifiedName? modulename, Scope scope)
    {
        foreach (var v in block.Parameters)
        {
            AppendDtor(context, block, scope, modulename, v.Tag.Name);
        }
    }

    private static BasicBlockBuilder EmitFunctionCall(IMethod method, LNode node, BasicBlockBuilder block, CompilerContext context, Scope scope, QualifiedName? moduleName)
    {
        //ToDo: continue implementing static function call in same type
        var type = (DescribedType)method.ParentType;
        var calleeName = node.Target;
        var methods = type.Methods;

        if (!methods.Any(_ => _.Name.ToString() == calleeName.ToString()))
        {
            type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(Names.FreeFunctions).Qualify(moduleName.Value)).FirstOrDefault();

            if (type == null)
            {
                context.AddError(node, new(ErrorID.CannotFindFunction, calleeName.ToString()));
            }
        }

        if (scope.TryGet<FunctionScopeItem>(calleeName.Name.Name, out var callee))
        {
            if (type.IsStatic && !callee.IsStatic)
            {
                context.AddError(node, $"A non static function '{calleeName.Name.Name}' cannot be called in a static function.");
                return block;
            }

            AppendCall(context, block, node, type.Methods, scope, moduleName.Value);
        }
        else
        {
            var suggestion = LevensteinDistance.Suggest(calleeName.Name.Name, type.Methods.Select(_ => _.Name.ToString()));

            context.AddError(node, new(ErrorID.CannotBeFoundDidYouMean, calleeName.Name.Name, suggestion));
        }

        return block;
    }

    private static void ConvertMethodBodies(CompilerContext context)
    {
        foreach (var bodyCompilation in context.BodyCompilations)
        {
            bodyCompilation.Method.Body =
                CompileBody(bodyCompilation.Function, context,
                bodyCompilation.Method, bodyCompilation.Modulename, bodyCompilation.Scope);
        }
    }
}