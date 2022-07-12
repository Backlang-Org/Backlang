using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public class DotNetAssembly : ITargetAssembly
{
    private readonly IAssembly _assembly;
    private readonly AssemblyContentDescription _description;
    private AssemblyDefinition _assemblyDefinition;

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
        foreach (DescribedType type in _assembly.Types)
        {
            var clrType = new TypeDefinition(type.FullName.Qualifier.ToString(),
                type.Name.ToString(), TypeAttributes.Class);

            ConvertCustomAttributes(type, clrType);

            if (type.IsPrivate)
            {
                clrType.Attributes |= TypeAttributes.NestedPrivate;
            }
            else if (type.IsProtected)
            {
                clrType.Attributes |= TypeAttributes.NestedFamily;
            }
            else
            {
                clrType.Attributes |= TypeAttributes.Public;
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
                        clrType.BaseType = Resolve(t.FullName);
                    }
                }
            }
            else
            {
                clrType.BaseType = _assemblyDefinition.MainModule.ImportReference(typeof(object));
            }

            foreach (DescribedField field in type.Fields)
            {
                var fieldType = Resolve(field.FieldType.FullName);
                var fieldDefinition = new FieldDefinition(field.Name.ToString(), FieldAttributes.Public, fieldType);

                var specialName = field.Attributes.GetAll().FirstOrDefault(_ => _.AttributeType.Name.ToString() == "SpecialNameAttribute");

                fieldDefinition.IsRuntimeSpecialName = specialName != null;
                fieldDefinition.IsSpecialName = specialName != null;
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
                }

                ConvertCustomAttributes(field, fieldDefinition);

                clrType.Fields.Add(fieldDefinition);
            }

            foreach (DescribedProperty property in type.Properties)
            {
                var propType = property.PropertyType;

                var clrProp = new PropertyDefinition(property.Name.ToString(), PropertyAttributes.None, Resolve(propType));

                var field = GeneratePropertyField(property);

                clrType.Fields.Add(field);

                var getter = GeneratePropertyGetter(property, field);
                var setter = GeneratePropertySetter(property, field);

                clrType.Methods.Add(getter);
                clrType.Methods.Add(setter);

                clrProp.GetMethod = getter;
                clrProp.SetMethod = setter;

                clrType.Properties.Add(clrProp);
            }

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

                if (m.Body != null)
                {
                    clrMethod.HasThis = false;

                    var variables = MethodBodyCompiler.Compile(m, clrMethod, _assemblyDefinition);
                    clrMethod.DebugInformation.Scope = new ScopeDebugInformation(clrMethod.Body.Instructions[0], clrMethod.Body.Instructions.Last());

                    foreach (var variable in variables)
                    {
                        clrMethod.DebugInformation.Scope.Variables.Add(new VariableDebugInformation(variable.definition, variable.name));
                    }
                }

                var attributes = m.Attributes.GetAll();
                if (attributes.Any())
                {
                    foreach (var attr in attributes)
                    {
                        if (attr.AttributeType.Name.ToString() == AccessModifierAttribute.AttributeName)
                        {
                            continue;
                        }

                        if (attr.AttributeType.Name.ToString() == "ExtensionAttribute")
                        {
                            var attrCtor = _assemblyDefinition.MainModule.ImportReference(typeof(ExtensionAttribute).GetConstructors().First());
                            var ca = new CustomAttribute(attrCtor);
                            clrType.IsBeforeFieldInit = false;
                            clrMethod.IsHideBySig = true;

                            clrMethod.CustomAttributes.Add(ca);
                        }
                    }
                }

                clrType.Methods.Add(clrMethod);
            }

            _assemblyDefinition.MainModule.Types.Add(clrType);
        }

        _assemblyDefinition.Write(output);

        output.Close();
    }

    private FieldDefinition GeneratePropertyField(DescribedProperty property)
    {
        var clrField = new FieldDefinition(@$"<{property.Name}>k__BackingField", FieldAttributes.Private, Resolve(property.PropertyType.FullName));

        clrField.CustomAttributes.Add(CompilerGeneratedAttribute());

        return clrField;
    }

    private MethodDefinition GeneratePropertyGetter(DescribedProperty property, FieldReference reference)
    {
        var clrMethod = new MethodDefinition(property.Getter.Name.ToString(),
                                GetMethodAttributes(property.Getter) | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                Resolve(property.PropertyType.FullName));

        clrMethod.CustomAttributes.Add(CompilerGeneratedAttribute());

        var ilProcessor = clrMethod.Body.GetILProcessor();

        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(OpCodes.Ldfld, reference);
        ilProcessor.Emit(OpCodes.Ret);

        return clrMethod;
    }

    private MethodDefinition GeneratePropertySetter(DescribedProperty property, FieldReference reference)
    {
        var clrMethod = new MethodDefinition(property.Setter.Name.ToString(),
                                GetMethodAttributes(property.Setter) | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                Resolve(new SimpleName("Void").Qualify("System")));

        clrMethod.CustomAttributes.Add(CompilerGeneratedAttribute());

        var param = new ParameterDefinition("value", ParameterAttributes.None, Resolve(property.PropertyType.FullName));
        clrMethod.Parameters.Add(param);

        var ilProcessor = clrMethod.Body.GetILProcessor();

        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(OpCodes.Ldarg_1);
        ilProcessor.Emit(OpCodes.Stfld, reference);
        ilProcessor.Emit(OpCodes.Ret);

        return clrMethod;
    }

    private CustomAttribute CompilerGeneratedAttribute()
    {
        var type = typeof(CompilerGeneratedAttribute).GetConstructors()[0];

        var attr = new CustomAttribute(_assemblyDefinition.MainModule.ImportReference(type));

        return attr;
    }

    private static MethodAttributes GetMethodAttributes(IMember member)
    {
        MethodAttributes attr = 0;

        var mod = member.GetAccessModifier();

        if (mod.HasFlag(AccessModifier.Private))
        {
            attr |= MethodAttributes.Private;
        }
        else if (mod.HasFlag(AccessModifier.Protected))
        {
            attr |= MethodAttributes.Family;
        }
        else if(mod.HasFlag(AccessModifier.Public))
        {
            attr |= MethodAttributes.Public;
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

    private void ConvertCustomAttributes(DescribedType type, TypeDefinition clrType)
    {
        foreach (DescribedAttribute attr in type.Attributes.GetAll().Where(_ => _ is DescribedAttribute))
        {
            if (attr.AttributeType.FullName.ToString() == "System.Runtime.InteropServices.StructLayoutAttribute")
            {
                ApplyStructLayout(clrType, attr);

                continue;
            }

            var attrType = _assemblyDefinition.ImportType(attr.AttributeType).Resolve();

            var attrCtor = attrType.Methods.First(_ => _.IsConstructor);
            var ca = new CustomAttribute(attrCtor);
            clrType.IsBeforeFieldInit = false;

            foreach (var arg in attr.ConstructorArguments)
            {
                ca.ConstructorArguments.Add(new CustomAttributeArgument(_assemblyDefinition.ImportType(arg.Type), arg.Value));
            }

            clrType.CustomAttributes.Add(ca);
        }
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