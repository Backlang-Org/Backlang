using Backlang.Codeanalysis.Core;
using Backlang.Contracts.Scoping.Items;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using System.Collections.Concurrent;
using System.Reflection;

namespace Backlang.Driver.Compiling.Stages.InitStages;

public sealed partial class InitStage : IHandler<CompilerContext, CompilerContext>
{
    private readonly Dictionary<string, ICompilationTarget> _targets = new();

    public InitStage()
    {
        AddTarget<DotNetTarget>();
    }

    public void InitTypeSystem(CompilerContext context)
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
            context.Environment = compilationTarget.Init(context);

            ImplicitTypeCastTable.InitCastMap(context.Environment);

            _targets.Clear();

            if (compilationTarget.HasIntrinsics)
            {
                AddIntrinsicType(context, compilationTarget.IntrinsicType);
            }
        }
        else
        {
            context.Messages.Add(Message.Error(new(ErrorID.TargetNotFound, context.Target.ToString())));
            return;
        }

        var consoleType = context.Binder.ResolveTypes(new SimpleName("Console").Qualify("System")).FirstOrDefault();

        context.writeMethods = consoleType?.Methods.Where(
            method => (method.Name.ToString() == "Write" || method.Name.ToString() == "WriteLine")
                && method.IsStatic
                && method.ReturnParameter.Type == context.Environment.Void);
    }

    private static void AddIntrinsicType(CompilerContext context, Type type)
    {
        var qualifier = Utils.QualifyNamespace(type.Namespace);
        var intrinsicAssembly = new DescribedAssembly(qualifier);

        var instrinsicsType = new DescribedType(
            new SimpleName(type.Name).Qualify(
                qualifier), intrinsicAssembly)
        {
            IsStatic = true
        };

        context.Binder.AddAssembly(context.Environment.Void.Parent.Assembly);

        var fields = type.GetFields().Where(_ => _.IsStatic);
        foreach (var field in fields)
        {
            AddIntrinsicEnum(context, field.FieldType, qualifier, intrinsicAssembly);
        }

        var methods = type.GetMethods().Where(_ => _.IsStatic);
        var toAdjustParameters = new ConcurrentBag<(MethodBase, DescribedMethod)>();

        intrinsicAssembly.AddType(instrinsicsType);
        context.Binder.AddAssembly(intrinsicAssembly);

        foreach (var method in methods)
        {
            ClrTypeEnvironmentBuilder.AddMethod(instrinsicsType, context.Binder, method, toAdjustParameters, method.Name.ToLower());
        }

        foreach (var toadjust in toAdjustParameters)
        {
            ClrTypeEnvironmentBuilder.ConvertParameter(toadjust.Item1.GetParameters(), toadjust.Item2, context);
        }
    }

    private static void AddIntrinsicEnum(CompilerContext context, Type fieldType, QualifiedName qualifier, DescribedAssembly intrinsicAssembly)
    {
        if (!fieldType.IsAssignableTo(typeof(Enum))) return;

        var type = new DescribedType(new SimpleName(fieldType.Name).Qualify(qualifier), intrinsicAssembly);

        type.AddAttribute(new IntrinsicAttribute("#Enum"));

        context.GlobalScope.Add(new TypeScopeItem { Name = "#" + type.Name, TypeInfo = type });

        foreach (var field in fieldType.GetFields())
        {
            var f = new DescribedField(type, new SimpleName(field.Name), true, type);

            type.AddField(f);
        }

        intrinsicAssembly.AddType(type);
    }

    private void InitPluginTargets(PluginContainer plugins)
    {
        if (plugins == null) return;

        foreach (var target in plugins?.Targets)
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