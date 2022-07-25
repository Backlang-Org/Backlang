using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Stages;

public sealed class InitTypeSystemStage : IHandler<CompilerContext, CompilerContext>
{
    private readonly Dictionary<string, ICompilationTarget> _targets = new();

    public InitTypeSystemStage()
    {
        AddTarget<DotNetTarget>();
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Binder = new TypeResolver();

        InitPluginTargets(context.Plugins);

        if (string.IsNullOrEmpty(context.Target))
        {
            context.Target = "dotnet";
        }

        if (context.OutputType == "dotnet")
        {
            context.OutputType = "Exe";
        }

        if (_targets.ContainsKey(context.Target))
        {
            var compilationTarget = _targets[context.Target];

            context.CompilationTarget = compilationTarget;
            context.Environment = compilationTarget.Init(context.Binder);

            if (compilationTarget.HasIntrinsics)
            {
                AddIntrinsicType(context.Binder, context.Environment, compilationTarget.IntrinsicType);
            }
        }

        var consoleType = context.Binder.ResolveTypes(new SimpleName("Console").Qualify("System")).FirstOrDefault();

        context.writeMethods = consoleType?.Methods.Where(
            method => method.Name.ToString() == "Write"
                && method.IsStatic
                && method.ReturnParameter.Type == context.Environment.Void);

        return await next.Invoke(context);
    }

    private static void AddIntrinsicType(TypeResolver binder, TypeEnvironment te, Type type)
    {
        var qualifier = ClrTypeEnvironmentBuilder.QualifyNamespace(type.Namespace);
        var intrinsicAssembly = new DescribedAssembly(qualifier);

        var instrinsicsType = new DescribedType(
            new SimpleName(type.Name).Qualify(
                qualifier), intrinsicAssembly)
        {
            IsStatic = true
        };

        binder.AddAssembly(te.Void.Parent.Assembly);

        var fields = type.GetFields().Where(_ => _.IsStatic);
        foreach (var field in fields)
        {
            AddIntrinsicEnum(field.FieldType, qualifier, intrinsicAssembly);
        }

        var methods = type.GetMethods().Where(_ => _.IsStatic);

        foreach (var method in methods)
        {
            var tmpBinder = new TypeResolver(binder.Assemblies.First());
            tmpBinder.AddAssembly(intrinsicAssembly);

            ClrTypeEnvironmentBuilder.AddMethod(instrinsicsType, tmpBinder, method, method.Name.ToLower());
        }

        intrinsicAssembly.AddType(instrinsicsType);

        binder.AddAssembly(intrinsicAssembly);
    }

    private static void AddIntrinsicEnum(Type fieldType, QualifiedName qualifier, DescribedAssembly intrinsicAssembly)
    {
        if (!fieldType.IsAssignableTo(typeof(Enum))) return;

        var type = new DescribedType(new SimpleName(fieldType.Name).Qualify(qualifier), intrinsicAssembly);

        type.AddAttribute(new IntrinsicAttribute("#Enum"));

        foreach (var field in fieldType.GetFields())
        {
            var f = new DescribedField(type, new SimpleName(field.Name), true, type);

            type.AddField(f);
        }

        intrinsicAssembly.AddType(type);
    }

    private void InitPluginTargets(PluginContainer plugins)
    {
        foreach (var target in plugins.Targets)
        {
            _targets.Add(target.Name, target);
        }
    }

    private void AddTarget<T>()
                    where T : ICompilationTarget, new()
    {
        var target = new T();
        _targets.Add(target.Name, target);
    }
}