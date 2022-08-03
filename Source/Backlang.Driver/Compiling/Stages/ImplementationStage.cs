using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Contracts;
using Backlang.Driver.Compiling.Scoping;
using Backlang.Driver.Compiling.Scoping.Items;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc;
using Loyc.Syntax;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Stages;

public enum ConditionalJumpKind
{
    NotEquals,
    Equals,
    True,
}

public sealed class ImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    private static ImmutableDictionary<Symbol, Type> LiteralTypeMap = new Dictionary<Symbol, Type>
    {
        [CodeSymbols.Bool] = typeof(bool),

        [CodeSymbols.String] = typeof(string),
        [CodeSymbols.Char] = typeof(char),

        [CodeSymbols.Int8] = typeof(byte),
        [CodeSymbols.Int16] = typeof(short),
        [CodeSymbols.UInt16] = typeof(ushort),
        [CodeSymbols.Int32] = typeof(int),
        [CodeSymbols.UInt32] = typeof(uint),
        [CodeSymbols.Int64] = typeof(long),
        [CodeSymbols.UInt64] = typeof(ulong),

        [Symbols.Float16] = typeof(Half),
        [Symbols.Float32] = typeof(float),
        [Symbols.Float64] = typeof(double),
    }.ToImmutableDictionary();

    public static MethodBody CompileBody(LNode function, CompilerContext context, IMethod method,
                QualifiedName? modulename)
    {
        var graph = Utils.CreateGraphBuilder();
        var block = graph.EntryPoint;

        Scope scope = new Scope(null);

        AppendBlock(function.Args[3], block, context, method, modulename, scope);

        return new MethodBody(
            method.ReturnParameter,
            new Parameter(method.ParentType),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    public static IType GetLiteralType(LNode value, TypeResolver resolver)
    {
        if (LiteralTypeMap.ContainsKey(value.Name))
        {
            return ClrTypeEnvironmentBuilder.ResolveType(resolver, LiteralTypeMap[value.Name]);
        }
        else if (value is IdNode id) { } //todo: symbol table
        else
        {
            return ClrTypeEnvironmentBuilder.ResolveType(resolver, value.Args[0].Value.GetType());
        }

        return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var modulename = Utils.GetModuleName(tree);

            foreach (var node in tree.Body)
            {
                CollectImplementations(context, node, modulename);
                ImplementDefaultsForStructs(context, node, modulename);
            }

            ConvertMethodBodies(context);
        }

        return await next.Invoke(context);
    }

    private static void ConvertMethodBodies(CompilerContext context)
    {
        foreach (var bodyCompilation in context.BodyCompilations)
        {
            bodyCompilation.method.Body =
                CompileBody(bodyCompilation.function, context,
                bodyCompilation.method, bodyCompilation.modulename);
        }
    }

    private static BasicBlockBuilder AppendBlock(LNode blkNode, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope)
    {
        foreach (var node in blkNode.Args)
        {
            if (!node.IsCall) continue;

            if (node.Calls(Symbols.Block))
            {
                if (node.ArgCount == 0) continue;

                block = AppendBlock(node, block.Graph.AddBasicBlock(), context, method, modulename, scope.CreateChildScope());
            }

            if (node.Calls(CodeSymbols.Var))
            {
                AppendVariableDeclaration(context, method, block, node, modulename, scope);
            }
            else if (node.Calls(CodeSymbols.If))
            {
                block = AppendIf(context, method, block, node, modulename, scope);
            }
            else if (node.Calls(CodeSymbols.While))
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
                    var exceptionType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(Exception));
                    var exceptionCtor = exceptionType.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == 1);

                    block.AppendInstruction(Instruction.CreateNewObject(exceptionCtor, new List<ValueTag> { msg }));
                }

                block.Flow = UnreachableFlow.Instance;
            }
            else if (node.Calls(Symbols.ColonColon))
            {
                var callee = node.Args[1];
                var typename = Utils.GetQualifiedName(node.Args[0]);

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

    private static BasicBlockBuilder AppendIf(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, (_, var condition, var body, var el)))
        {
            var ifBlock = block.Graph.AddBasicBlock(Utils.NewLabel("if"));
            AppendBlock(body, ifBlock, context, method, modulename, scope.CreateChildScope());

            if (el != LNode.Missing)
            {
                var elseBlock = block.Graph.AddBasicBlock(Utils.NewLabel("else"));
                AppendBlock(el, elseBlock, context, method, modulename, scope.CreateChildScope());
            }

            var if_condition = block.Graph.AddBasicBlock(Utils.NewLabel("if_condition"));
            if (condition.Calls(CodeSymbols.Bool))
            {
                if_condition.Flow = new JumpConditionalFlow(ifBlock, ConditionalJumpKind.True);
            }
            else
            {
                AppendExpression(if_condition, condition, context.Environment.Boolean, method);
                if_condition.Flow = new JumpConditionalFlow(ifBlock, ConditionalJumpKind.Equals);
            }

            block.Flow = new JumpFlow(if_condition);

            var after = block.Graph.AddBasicBlock(Utils.NewLabel("after"));
            ifBlock.Flow = new JumpFlow(after);

            return after;
        }

        return null;
    }

    private static BasicBlockBuilder AppendWhile(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, var condition, var body))
        {
            var while_start = block.Graph.AddBasicBlock(Utils.NewLabel("while_start"));
            AppendBlock(body, while_start, context, method, modulename, scope.CreateChildScope());

            var while_condition = block.Graph.AddBasicBlock(Utils.NewLabel("while_condition"));
            AppendExpression(while_condition, condition, context.Environment.Boolean, method);
            while_condition.Flow = new JumpFlow(while_start);

            var while_end = block.Graph.AddBasicBlock(Utils.NewLabel("while_end"));
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

    private static IMethod GetMatchingMethod(CompilerContext context, List<IType> argTypes, IEnumerable<IMethod> methods, string methodname)
    {
        foreach (var m in methods)
        {
            if (m.Name.ToString() != methodname) continue;

            if (m.Parameters.Count == argTypes.Count)
            {
                if (MatchesParameters(m, argTypes))
                    return m;
            }
        }

        return null;
    }

    private static bool MatchesParameters(IMethod method, List<IType> argTypes)
    {
        var methodParams = string.Join(',', method.Parameters.Select(_ => _.Type.FullName.ToString()));
        var monocecilParams = string.Join(',', argTypes.Select(_ => _.FullName.ToString()));

        return methodParams.Equals(monocecilParams, StringComparison.Ordinal);
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

    private static void AppendVariableDeclaration(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        var decl = node.Args[1];

        var name = Utils.GetQualifiedName(node.Args[0]);
        var elementType = TypeInheritanceStage.ResolveTypeWithModule(node.Args[0], context, modulename.Value, name);

        var varname = decl.Args[0].Name.Name;
        if (!block.Parameters.Select(_ => _.Tag.Name).Contains(varname))
        {
            block.AppendParameter(new BlockParameter(elementType, varname));
        }
        else
        {
            context.AddError(decl.Args[0], $"{varname} already declared");
        }

        scope.Add(new VariableScopeItem { Name = varname, IsMutable = false }); // TODO: Check Mutable for AppendVariableDeclaration

        AppendExpression(block, decl.Args[1], elementType, method);

        block.AppendInstruction(Instruction.CreateAlloca(elementType));
    }

    private static NamedInstructionBuilder AppendExpression(BasicBlockBuilder block, LNode node, IType elementType, IMethod method)
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

    private static Instruction ConvertConstant(IType elementType, object value)
    {
        Constant constant;
        switch (value)
        {
            case uint v:
                constant = new IntegerConstant(v);
                break;

            case int v:
                constant = new IntegerConstant(v);
                break;

            case long v:
                constant = new IntegerConstant(v);
                break;

            case ulong v:
                constant = new IntegerConstant(v);
                break;

            case byte v:
                constant = new IntegerConstant(v);
                break;

            case short v:
                constant = new IntegerConstant(v);
                break;

            case ushort v:
                constant = new IntegerConstant(v);
                break;

            case float v:
                constant = new Float32Constant(v);
                break;

            case double v:
                constant = new Float64Constant(v);
                break;

            case string v:
                constant = new StringConstant(v);
                break;

            case char v:
                constant = new IntegerConstant(v);
                break;

            case bool v:
                constant = BooleanConstant.Create(v);
                break;

            default:
                if (value == null)
                {
                    constant = NullConstant.Instance;
                }
                else
                {
                    if (elementType.Attributes.Contains(IntrinsicAttribute.GetIntrinsicAttributeType("#Enum")))
                    {
                        constant = new EnumConstant(value, elementType);
                    }
                    else
                    {
                        constant = null;
                    }
                }

                break;
        }

        return Instruction.CreateConstant(constant,
                                           elementType);
    }

    private static void ImplementDefaultsForStructs(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == CodeSymbols.Struct)) return;

        var name = st.Args[0].Name;
        var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(modulename)).FirstOrDefault();

        // toString method
        if (!type.Methods.Any(_ => _.Name.ToString() == "ToString" && _.Parameters.Count == 0))
        {
            Generator.GenerateToString(context, type);
        }

        // default constructor
        if (!type.Methods.Any(_ => _.Name.ToString() == "new" && _.Parameters.Count == type.Fields.Count))
        {
            Generator.GenerateDefaultCtor(context, type);
        }
    }

    private static void CollectImplementations(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == Symbols.Implementation)) return;

        var typenode = st.Args[0].Args[0].Args[0].Args[0];
        var fullname = Utils.GetQualifiedName(typenode);
        var targetType = (DescribedType)TypeInheritanceStage.ResolveTypeWithModule(typenode, context, modulename, fullname);

        var body = st.Args[0].Args[1].Args;

        foreach (var node in body)
        {
            if (node.Name == CodeSymbols.Fn)
            {
                if (targetType.Parent.Assembly == context.Assembly)
                {
                    var fn = TypeInheritanceStage.ConvertFunction(context, targetType, node, modulename);
                    targetType.AddMethod(fn);
                }
                else
                {
                    var fn = TypeInheritanceStage.ConvertFunction(context, context.ExtensionsType, node, modulename);

                    fn.IsStatic = true;

                    var thisParameter = new Parameter(targetType, "this");
                    var param = (IList<Parameter>)fn.Parameters;

                    param.Insert(0, thisParameter);

                    var extType = ClrTypeEnvironmentBuilder
                        .ResolveType(context.Binder, typeof(ExtensionAttribute));

                    fn.AddAttribute(new DescribedAttribute(extType));

                    context.ExtensionsType.AddMethod(fn);
                }
            }
        }
    }
}