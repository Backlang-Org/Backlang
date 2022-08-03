using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Contracts.Scoping.Items;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Stages;

public sealed partial class TypeInheritanceStage : IHandler<CompilerContext, CompilerContext>
{
    public static DescribedBodyMethod ConvertFunction(CompilerContext context, DescribedType type,
        LNode function, QualifiedName modulename, Scope parentScope, string methodName = null, bool hasBody = true)
    {
        if (methodName == null) methodName = Utils.GetMethodName(function);

        var returnType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void));

        var method = new DescribedBodyMethod(type,
            new QualifiedName(methodName).FullyUnqualifiedName,
            function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), returnType);

        Utils.SetAccessModifier(function, method);

        ConvertAnnotations(function, method, context, modulename,
            AttributeTargets.Method, (attr, t) => ((DescribedBodyMethod)t).AddAttribute(attr));

        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Operator)))
        {
            method.AddAttribute(new DescribedAttribute(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(SpecialNameAttribute))));
        }
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Override)))
        {
            method.IsOverride = true;
        }
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Extern)))
        {
            method.IsExtern = true;
        }
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Abstract)))
        {
            method.AddAttribute(FlagAttribute.Abstract);
        }

        var scope = parentScope.CreateChildScope();

        AddParameters(method, function, context, modulename, scope);
        SetReturnType(method, function, context, modulename);

        if (methodName == ".ctor")
        {
            method.IsConstructor = true;
        }
        else if (methodName == ".dtor")
        {
            method.IsDestructor = true;
        }

        var functionItem = new FunctionScopeItem { Name = methodName, SubScope = scope, Method = method };

        if (hasBody)
        {
            context.BodyCompilations.Add(new(function, context, method, modulename, scope));
        }

        if (parentScope.Add(functionItem))
        {
            return method;
        }

        context.AddError(function, "Function '" + method.FullName + "' is already defined.");
        return null;
    }

    private static void AddParameters(DescribedBodyMethod method, LNode function, CompilerContext context, QualifiedName modulename, Scope scope)
    {
        var param = function.Args[2];

        foreach (var p in param.Args)
        {
            var pa = ConvertParameter(p, context, modulename);
            if (scope.Add(new ParameterScopeItem { Name = pa.FullName.ToString(), Parameter = pa }))
            {
                method.AddParameter(pa);
            }
            else
            {
                context.AddError(param, $"Function Parameter {pa.FullName.ToString()} was already defined.");
            }
        }
    }

    private static void ConvertFreeFunction(CompilerContext context, LNode node, QualifiedName modulename, Scope scope)
    {
        DescribedType type;

        if (!context.Assembly.Types.Any(_ => _.FullName.FullName == $".{Names.ProgramClass}"))
        {
            type = new DescribedType(new SimpleName(Names.ProgramClass).Qualify(string.Empty), context.Assembly);
            type.IsStatic = true;
            type.IsPublic = true;

            context.Assembly.AddType(type);
        }
        else
        {
            type = (DescribedType)context.Assembly.Types.First(_ => _.FullName.FullName == $".{Names.ProgramClass}");
        }

        string methodName = Utils.GetMethodName(node);
        if (methodName == "main") methodName = "Main";

        var method = ConvertFunction(context, type, node, modulename, scope, methodName: methodName);

        if (method != null) type.AddMethod(method);
    }

    private static Parameter ConvertParameter(LNode p, CompilerContext context, QualifiedName modulename)
    {
        var ptype = p.Args[0].Args[0].Args[0];

        var type = ResolveTypeWithModule(ptype, context, modulename);
        var assignment = p.Args[1];

        var name = assignment.Args[0].Name;

        var param = new Parameter(type, name.ToString());

        if (!assignment.Args[1].Args.IsEmpty)
        {
            param.HasDefault = true;
            param.DefaultValue = assignment.Args[1].Args[0].Value;
        }

        return param;
    }

    private static void SetReturnType(DescribedBodyMethod method, LNode function, CompilerContext context, QualifiedName modulename)
    {
        var retType = function.Args[0];

        var rtype = ResolveTypeWithModule(retType, context, modulename);

        method.ReturnParameter = new Parameter(rtype);
    }
}