using Backlang.Contracts.Scoping.Items;
using Flo;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Stages;

public sealed partial class TypeInheritanceStage : IHandler<CompilerContext, CompilerContext>
{
    public static DescribedBodyMethod ConvertFunction(CompilerContext context, DescribedType type,
        LNode function, QualifiedName modulename, Scope parentScope, string methodName = null, bool hasBody = true)
    {
        if (methodName == null)
        {
            methodName = ConversionUtils.GetMethodName(function);
        }

        var returnType = Utils.ResolveType(context.Binder, typeof(void));

        var method = new DescribedBodyMethod(type,
            new QualifiedName(methodName).FullyUnqualifiedName,
            function.Attrs.Contains(LNode.Id(CodeSymbols.Static)) || type.Name.ToString() == Names.FreeFunctions,
            returnType);

        ConversionUtils.SetAccessModifier(function, method);

        ConvertAnnotations(function, method, context, modulename,
            AttributeTargets.Method, (attr, t) => ((DescribedBodyMethod)t).AddAttribute(attr));

        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Operator)))
        {
            method.AddAttribute(
                new DescribedAttribute(Utils.ResolveType(context.Binder, typeof(SpecialNameAttribute))));
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

        if (methodName == ".ctor")
        {
            method.IsConstructor = true;
        }
        else if (methodName == ".dtor")
        {
            method.IsDestructor = true;
        }

        var functionItem = new FunctionScopeItem
        {
            Name = methodName, SubScope = scope, Overloads = new List<IMethod> { method }
        };

        if (hasBody)
        {
            context.BodyCompilations.Add(new MethodBodyCompilation(function, context, method, modulename, scope));
        }

        if (parentScope.Add(functionItem))
        {
            return method;
        }

        context.AddError(function, "Function '" + method.FullName + "' is already defined.");
        return null;
    }

    private static void AddParameters(DescribedBodyMethod method, LNode function, CompilerContext context,
        QualifiedName modulename, Scope scope)
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

        if (!context.Assembly.Types.Any(_ =>
                _.FullName.ToString() == new SimpleName(Names.FreeFunctions).Qualify(modulename).ToString()))
        {
            type = new DescribedType(new SimpleName(Names.FreeFunctions).Qualify(modulename), context.Assembly)
            {
                IsStatic = true, IsPublic = true
            };

            context.Assembly.AddType(type);
            var tr = new TypeResolver();

            foreach (var a in context.Binder.Assemblies)
            {
                tr.AddAssembly(a);
            }

            context.Binder = tr;
        }
        else
        {
            type = (DescribedType)context.Assembly.Types.First(_ => _.Name.ToString() == Names.FreeFunctions);
        }

        var methodName = ConversionUtils.GetMethodName(node);
        if (methodName == "main")
        {
            methodName = "Main";
        }

        var method = ConvertFunction(context, type, node, modulename, scope, methodName);

        if (method != null)
        {
            type.AddMethod(method);
        }
    }

    private static Parameter ConvertParameter(LNode p, CompilerContext context, QualifiedName modulename)
    {
        var ptype = p.Args[0];
        var assignment = p.Args[1];

        var type = ResolveTypeWithModule(ptype, context, modulename);

        var name = assignment.Args[0].Name;

        var param = new Parameter(type, name.ToString());

        if (!assignment.Args[1].Args.IsEmpty)
        {
            param.HasDefault = true;
            param.DefaultValue = assignment.Args[1].Args[0].Value;
        }

        return param;
    }
}