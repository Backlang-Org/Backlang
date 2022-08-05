using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public record struct MethodBodyCompilation(DescribedBodyMethod DescribedMethod,
    MethodDefinition ClrMethod, TypeDefinition ClrType);

public class DotNetAssembly : ITargetAssembly
{
    private static readonly List<MethodBodyCompilation> _methodBodyCompilations = new();
    private readonly IAssembly _assembly;
    private readonly AssemblyContentDescription _description;
    private readonly List<(TypeDefinition definition, QualifiedName name)> _needToAdjust = new();
    private readonly AssemblyDefinition _assemblyDefinition;

    public DotNetAssembly(AssemblyContentDescription description)
    {
        _assembly = description.Assembly;

        var name = new AssemblyNameDefinition(_assembly.FullName.ToString(),
            new Version(1, 0));

        _assemblyDefinition = AssemblyDefinition.CreateAssembly(name, description.Assembly.Name.ToString(), ModuleKind.Dll);

        _description = description;

        SetTargetFramework();

        var console = typeof(Console).Assembly.GetName();
        _assemblyDefinition.MainModule.AssemblyReferences.Add(AssemblyNameReference.Parse(console.FullName));
    }

    public void WriteTo(Stream output)
    {
        var typeMap = new ConcurrentDictionary<DescribedType, TypeDefinition>();

        foreach (var type in _assembly.Types.Cast<DescribedType>())
        {
            var clrType = new TypeDefinition(type.FullName.Slice(0, type.FullName.PathLength - 1).FullName.ToString(),
               type.Name.ToString(), TypeAttributes.Class);
            _assemblyDefinition.MainModule.Types.Add(clrType);

            typeMap.AddOrUpdate(type, (_) => clrType, (_, __) => clrType);
        }

        Parallel.ForEachAsync(typeMap.AsParallel(), (KeyValuePair, ct) => {
            ConvertCustomAttributes(KeyValuePair.Key, KeyValuePair.Value);
            ApplyModifiers(KeyValuePair.Key, KeyValuePair.Value);
            SetBaseType(KeyValuePair.Key, KeyValuePair.Value);
            ConvertFields(KeyValuePair.Key, KeyValuePair.Value);
            ConvertProperties(KeyValuePair.Key, KeyValuePair.Value);
            ConvertMethods(KeyValuePair.Key, KeyValuePair.Value);

            return ValueTask.CompletedTask;
        }).Wait();

        AdjustBaseTypesAndInterfaces();

        CompileBodys();

        Parallel.ForEach(_assembly.Attributes.GetAll().Where(_ => _ is EmbeddedResourceAttribute).Cast<EmbeddedResourceAttribute>(), (er) => {
            var err = new EmbeddedResource(er.Name, ManifestResourceAttributes.Public, File.OpenRead(er.Filename));

            _assemblyDefinition.MainModule.Resources.Add(err);
        });

        _assemblyDefinition.Write(output);

        output.Close();
    }

    private static void ApplyModifiers(DescribedType type, TypeDefinition clrType)
    {
        if (type.IsPrivate)
        {
            clrType.Attributes |= TypeAttributes.NestedPrivate;
        }
        else if (type.IsProtected)
        {
            clrType.Attributes |= TypeAttributes.NestedFamily;
        }
        else if (type.IsPublic)
        {
            clrType.Attributes |= TypeAttributes.Public;
        }
        else
        {
            // here also 'internal' is used, because 'internal' doesnt need any attribute.
        }
        if (type.IsStatic)
        {
            clrType.Attributes |= TypeAttributes.Abstract;
            clrType.Attributes |= TypeAttributes.Sealed;
        }

        if (type.IsAbstract)
        {
            clrType.Attributes |= TypeAttributes.Abstract;
        }

        clrType.IsInterface = type.IsInterfaceType;
    }

    private static MethodAttributes GetMethodAttributes(IMember member)
    {
        MethodAttributes attr = 0;

        var mod = member.GetAccessModifier();

        if (mod.HasFlag(AccessModifier.Public))
        {
            attr |= MethodAttributes.Public;
        }
        else if (mod.HasFlag(AccessModifier.Protected))
        {
            attr |= MethodAttributes.Family;
        }
        else if (mod.HasFlag(AccessModifier.Private))
        {
            attr |= MethodAttributes.Private;
        }
        else
        {
            attr |= MethodAttributes.Assembly;
        }

        return attr;
    }

    private static void ApplyStructLayout(TypeDefinition clrType, DescribedAttribute attr)
    {
        var layout = (LayoutKind)attr.ConstructorArguments[0].Value;

        switch (layout)
        {
            case LayoutKind.Sequential:
                clrType.IsSequentialLayout = true;
                break;

            case LayoutKind.Explicit:
                clrType.IsExplicitLayout = true;
                break;

            default:
                break;
        }
    }

    private FieldDefinition GeneratePropertyField(DescribedProperty property)
    {
        var clrField = new FieldDefinition(@$"<{property.Name}>k__BackingField", FieldAttributes.Private, Resolve(property.PropertyType.FullName));

        clrField.CustomAttributes.Add(GetCompilerGeneratedAttribute());

        return clrField;
    }

    private MethodDefinition GeneratePropertyGetter(DescribedProperty property, FieldReference reference)
    {
        var clrMethod = new MethodDefinition(property.Getter.Name.ToString(),
                                GetMethodAttributes(property.Getter) | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                Resolve(property.PropertyType.FullName));

        clrMethod.CustomAttributes.Add(GetCompilerGeneratedAttribute());

        var ilProcessor = clrMethod.Body.GetILProcessor();

        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(OpCodes.Ldfld, reference);
        ilProcessor.Emit(OpCodes.Ret);

        return clrMethod;
    }

    private CustomAttribute GetCompilerGeneratedAttribute()
    {
        var type = typeof(CompilerGeneratedAttribute).GetConstructors()[0];

        var attr = new CustomAttribute(_assemblyDefinition.MainModule.ImportReference(type));

        return attr;
    }

    private MethodDefinition GeneratePropertySetter(DescribedProperty property, FieldReference reference, DescribedPropertyMethod propMethod, bool isInitOnly)
    {
        var clrMethod = new MethodDefinition(propMethod.Name.ToString(),
                                GetMethodAttributes(propMethod) | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                Resolve(new SimpleName("Void").Qualify("System")));

        clrMethod.CustomAttributes.Add(GetCompilerGeneratedAttribute());

        if (isInitOnly)
        {
            clrMethod.ReturnType = new RequiredModifierType(_assemblyDefinition.MainModule.ImportReference(typeof(System.Runtime.CompilerServices.IsExternalInit)), clrMethod.ReturnType);
        }

        var param = new ParameterDefinition("value", ParameterAttributes.None, Resolve(property.PropertyType.FullName));
        clrMethod.Parameters.Add(param);

        var ilProcessor = clrMethod.Body.GetILProcessor();

        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(OpCodes.Ldarg_1);
        ilProcessor.Emit(OpCodes.Stfld, reference);
        ilProcessor.Emit(OpCodes.Ret);

        return clrMethod;
    }

    private void ConvertProperties(DescribedType type, TypeDefinition clrType)
    {
        foreach (DescribedProperty property in type.Properties)
        {
            var propType = property.PropertyType;

            var clrProp = new PropertyDefinition(property.Name.ToString(), PropertyAttributes.None, Resolve(propType));

            var field = GeneratePropertyField(property);

            clrType.Fields.Add(field);

            if (property.HasGetter)
            {
                var getter = GeneratePropertyGetter(property, field);
                clrType.Methods.Add(getter);
                clrProp.GetMethod = getter;
            }
            if (property.HasSetter)
            {
                var setter = GeneratePropertySetter(property, field, property.Setter, false);
                clrType.Methods.Add(setter);
                clrProp.SetMethod = setter;
            }
            else if (property.HasInitOnlySetter)
            {
                var initOnlySetter = GeneratePropertySetter(property, field, property.InitOnlySetter, true);
                clrType.Methods.Add(initOnlySetter);
                clrProp.SetMethod = initOnlySetter;
            }

            clrType.Properties.Add(clrProp);
        }
    }

    private void AdjustBaseTypesAndInterfaces()
    {
        foreach (var baseType in _needToAdjust)
        {
            var type = Resolve(baseType.name).Resolve();

            if (type.IsInterface)
            {
                baseType.definition.Interfaces.Add(new InterfaceImplementation(type));
            }
            else
            {
                baseType.definition.BaseType = type;
            }
        }
    }

    private void ConvertMethods(DescribedType type, TypeDefinition clrType)
    {
        foreach (DescribedBodyMethod m in type.Methods)
        {
            var returnType = m.ReturnParameter.Type;
            var clrMethod = GetMethodDefinition(m, returnType);

            if (m.IsOverride)
            {
                clrMethod.IsHideBySig = true;
                clrMethod.IsVirtual = true;
            }

            clrMethod.IsAbstract = m.IsAbstract;
            clrMethod.IsHideBySig = m.Owns(Attributes.Mutable);

            if (m.IsStatic)
            {
                clrMethod.HasThis = false;
            }

            if (m.IsConstructor)
            {
                clrMethod.IsRuntimeSpecialName = true;
                clrMethod.IsSpecialName = true;
                clrMethod.IsHideBySig = true;

                clrMethod.HasThis = true;

                clrMethod.Name = ".ctor";
            }
            else if (m.IsDestructor)
            {
                clrMethod.Overrides.Add(_assemblyDefinition.MainModule.ImportReference(typeof(object).GetMethod("Finalize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)));
                clrMethod.Name = "Finalize";
                clrMethod.IsVirtual = true;
                clrMethod.IsFamily = true;
                clrMethod.IsHideBySig = true;
            }

            if (m.Body != null)
            {
                _methodBodyCompilations.Add(new(m, clrMethod, clrType));
            }

            ConvertCustomAttributes(clrType, m, clrMethod);

            clrType.Methods.Add(clrMethod);
        }
    }

    private void CompileBodys()
    {
        Parallel.ForEach(_methodBodyCompilations, bodyCompilation => {
            var variables =
                                MethodBodyCompiler.Compile(bodyCompilation.DescribedMethod, bodyCompilation.ClrMethod, _assemblyDefinition, bodyCompilation.ClrType);

            bodyCompilation.ClrMethod.DebugInformation.Scope =
                new ScopeDebugInformation(bodyCompilation.ClrMethod.Body.Instructions[0],
                    bodyCompilation.ClrMethod.Body.Instructions.Last());

            foreach (var variable in variables)
            {
                bodyCompilation.ClrMethod.DebugInformation.Scope.Variables.Add(new VariableDebugInformation(variable.Value, variable.Key));
            }
        });
    }

    private void ConvertCustomAttributes(TypeDefinition clrType, DescribedBodyMethod m, MethodDefinition clrMethod)
    {
        var attributes = m.Attributes.GetAll().Where(_ => _ is DescribedAttribute);
        if (attributes.Any())
        {
            foreach (DescribedAttribute attr in attributes)
            {
                if (attr.AttributeType.Name.ToString() == AccessModifierAttribute.AttributeName)
                {
                    continue;
                }

                ConvertCustomAttribute(clrType, clrMethod, attr);
            }
        }
    }

    private void ConvertCustomAttribute(TypeDefinition clrType, MethodDefinition clrMethod, DescribedAttribute attr)
    {
        var attrType = _assemblyDefinition.ImportType(attr.AttributeType).Resolve();
        var attrCtor = attrType.Methods.FirstOrDefault(_ => _.IsConstructor && attr.ConstructorArguments.Count == _.Parameters.Count);

        var ca = new CustomAttribute(_assemblyDefinition.MainModule.ImportReference(attrCtor));
        clrType.IsBeforeFieldInit = false;
        clrMethod.IsHideBySig = true;

        foreach (var cattr in attr.ConstructorArguments)
        {
            ca.ConstructorArguments.Add(
                new CustomAttributeArgument(_assemblyDefinition.ImportType(cattr.Type), cattr.Value));
        }

        clrMethod.CustomAttributes.Add(ca);
    }

    private void ConvertFields(DescribedType type, TypeDefinition clrType)
    {
        foreach (DescribedField field in type.Fields)
        {
            var fieldType = Resolve(field.FieldType.FullName);
            var fieldDefinition = new FieldDefinition(field.Name.ToString(), FieldAttributes.Public, fieldType);

            fieldDefinition.IsStatic = field.IsStatic;
            fieldDefinition.IsInitOnly = !field.Owns(Attributes.Mutable);

            if (clrType.IsEnum || field.InitialValue != null)
            {
                fieldDefinition.Constant = field.InitialValue;

                if (field.Name.ToString() != "value__")
                {
                    fieldDefinition.IsRuntimeSpecialName = false;
                    fieldDefinition.IsSpecialName = false;
                    fieldDefinition.IsLiteral = true;
                }
                else
                {
                    fieldDefinition.IsInitOnly = true;
                }
            }

            ConvertCustomAttributes(field, fieldDefinition);

            clrType.Fields.Add(fieldDefinition);
        }
    }

    private void SetBaseType(DescribedType type, TypeDefinition clrType)
    {
        if (type.BaseTypes.Any())
        {
            foreach (var t in type.BaseTypes)
            {
                if (t.Name.ToString() == "ValueType")
                {
                    clrType.BaseType = _assemblyDefinition.MainModule.ImportReference(typeof(ValueType));

                    //clrType.ClassSize = 1;
                    //clrType.PackingSize = 0;
                }
                else
                {
                    AddBaseType(clrType, t.FullName);
                }
            }
        }
        else
        {
            clrType.BaseType = _assemblyDefinition.MainModule.ImportReference(typeof(object));
        }
    }

    private void AddBaseType(TypeDefinition clrType, QualifiedName fullName)
    {
        _needToAdjust.Add((clrType, fullName));
    }

    private void ConvertCustomAttributes(DescribedType type, TypeDefinition clrType)
    {
        foreach (DescribedAttribute attr in type.Attributes.GetAll().Where(_ => _ is DescribedAttribute))
        {
            if (attr.AttributeType.FullName.ToString() == "System.Runtime.InteropServices.StructLayoutAttribute")
            {
                ApplyStructLayout(clrType, attr);

                continue;
            }

            ConvertCustomAttribute(clrType, attr);
        }
    }

    private void ConvertCustomAttribute(TypeDefinition clrType, DescribedAttribute attr)
    {
        var attrType = _assemblyDefinition.ImportType(attr.AttributeType).Resolve();
        var attrCtor = attrType.Methods
                                        .FirstOrDefault(_ =>
                                            _.IsConstructor
                                            && attr.ConstructorArguments.Count == _.Parameters.Count);

        var ca = new CustomAttribute(_assemblyDefinition.MainModule.ImportReference(attrCtor));
        clrType.IsBeforeFieldInit = false;

        foreach (var cattr in attr.ConstructorArguments)
        {
            ca.ConstructorArguments.Add(
                new CustomAttributeArgument(_assemblyDefinition.ImportType(cattr.Type), cattr.Value));
        }

        clrType.CustomAttributes.Add(ca);
    }

    private void ConvertCustomAttributes(DescribedField field, FieldDefinition clrField)
    {
        foreach (DescribedAttribute attr in field.Attributes.GetAll().Where(_ => _ is DescribedAttribute))
        {
            if (attr.AttributeType.FullName.ToString() == typeof(FieldOffsetAttribute).FullName)
            {
                clrField.Offset = (int)attr.ConstructorArguments[0].Value;
                continue;
            }
            else if (attr.AttributeType.Name.ToString() == "SpecialNameAttribute")
            {
                clrField.IsRuntimeSpecialName = true;
                clrField.IsSpecialName = true;
                continue;
            }

            var attrType = _assemblyDefinition.ImportType(attr.AttributeType).Resolve();

            var attrCtor = attrType.Methods.First(_ => _.IsConstructor);
            var ca = new CustomAttribute(attrCtor);

            foreach (var arg in attr.ConstructorArguments)
            {
                ca.ConstructorArguments.Add(new CustomAttributeArgument(_assemblyDefinition.ImportType(arg.Type), arg.Value));
            }

            clrField.CustomAttributes.Add(ca);
        }
    }

    private MethodDefinition GetMethodDefinition(DescribedBodyMethod m, IType returnType)
    {
        var clrMethod = new MethodDefinition(m.Name.ToString(),
                                GetMethodAttributes(m),
                               Resolve(returnType == null ? new SimpleName("Void").Qualify("System") : returnType.FullName));

        if (m == _description.EntryPoint)
        {
            _assemblyDefinition.EntryPoint = clrMethod;
        }

        foreach (var p in m.Parameters)
        {
            var param = new ParameterDefinition(p.Name.ToString(), ParameterAttributes.None, Resolve(p.Type));
            if (p.HasDefault)
            {
                param.Constant = p.DefaultValue;
                param.IsOptional = true;
            }
            clrMethod.Parameters.Add(param);
        }

        clrMethod.IsStatic = m.IsStatic;
        if (m.IsConstructor)
        {
            clrMethod.IsRuntimeSpecialName = true;
            clrMethod.IsSpecialName = true;
            clrMethod.Name = ".ctor";
            clrMethod.IsStatic = false;
        }
        return clrMethod;
    }

    private TypeReference Resolve(IType dtype)
    {
        var resolvedType = Resolve(dtype.FullName);
        if (resolvedType.HasGenericParameters)
        {
            var genericType = new GenericInstanceType(resolvedType);

            foreach (var gp in dtype.GenericParameters)
            {
                if (gp.Name.ToString() == "#" || gp.Name.ToString().StartsWith("T")
                    || gp.Name.ToString().StartsWith("TResult")) continue;

                var resolvedGeneric = Resolve(gp.Name.Qualify("System"));
                genericType.GenericArguments.Add(resolvedGeneric);
            }

            if (dtype.Name.ToString().Contains("Func"))
            {
                genericType.GenericArguments.Add(_assemblyDefinition.MainModule.ImportReference(typeof(object)));
            }

            return genericType;
        }

        return resolvedType;
    }

    private TypeReference Resolve(QualifiedName name)
    {
        return _assemblyDefinition.ImportType(name);
    }

    private void SetTargetFramework()
    {
        var tf = _assemblyDefinition.MainModule.ImportReference(typeof(TargetFrameworkAttribute).GetConstructors().First());

        var item = new CustomAttribute(tf);
        item.ConstructorArguments.Add(
            new CustomAttributeArgument(_assemblyDefinition.MainModule.ImportReference(typeof(string)), ".NETCoreApp,Version=v7.0"));

        _assemblyDefinition.CustomAttributes.Add(item);
    }
}