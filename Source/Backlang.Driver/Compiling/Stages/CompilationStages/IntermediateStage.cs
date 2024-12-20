﻿using Backlang.Contracts.Scoping.Items;
using Backlang.Core.CompilerService;
using Flo;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Globalization;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public sealed class IntermediateStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context,
        Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Assembly = new DescribedAssembly(new QualifiedName(context.Options.OutputFilename.Replace(".dll", "")));

        foreach (var tree in context.Trees)
        {
            var modulename = ConversionUtils.GetModuleName(tree);
            var importetNamespaces = GetNamespaceImports(tree, context);
            var imports = new NamespaceImports();

            foreach (var importStatement in importetNamespaces)
            {
                imports.ImportNamespace(importStatement, context);
            }

            context.FileScope.ImportetNamespaces.Add(tree.Document.FileName, imports);

            foreach (var st in tree.Body)
            {
                if (st.Calls(CodeSymbols.UsingStmt))
                {
                    AddTypeAlias(st, context.GlobalScope, context, modulename);
                }
                else if (st.Calls(CodeSymbols.Struct) || st.Calls(CodeSymbols.Class) || st.Calls(CodeSymbols.Interface))
                {
                    ConvertTypeOrInterface(context, st, modulename, context.GlobalScope);
                }
                else if (st.Calls(Symbols.UnitDecl))
                {
                    ConvertUnitDeclaration(context, st, modulename, context.GlobalScope);
                }
                else if (st.Calls(CodeSymbols.Enum))
                {
                    ConvertEnum(context, st, modulename);
                }
                else if (st.Calls(Symbols.DiscriminatedUnion))
                {
                    ConvertDiscriminatedUnion(context, st, modulename);
                }
            }
        }

        context.Binder.AddAssembly(context.Assembly);

        return await next.Invoke(context);
    }

    private static void ConvertEnum(CompilerContext context, LNode @enum, QualifiedName modulename)
    {
        if (@enum is var (_, (_, nameNode, typeNode, membersNode)))
        {
            var name = nameNode.Name;

            var type = new DescribedType(new SimpleName(name.Name).Qualify(modulename), context.Assembly);
            type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("Enum").Qualify("System"))[0]);

            type.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

            context.Assembly.AddType(type);
        }
    }

    private static void ConvertTypeOrInterface(CompilerContext context, LNode st, QualifiedName modulename, Scope scope)
    {
        var name = st.Args[0].Name;

        var type = new DescribedType(new SimpleName(name.Name).Qualify(modulename), context.Assembly);
        if (st.Name == CodeSymbols.Struct)
        {
            type.AddBaseType(
                context.Binder.ResolveTypes(new SimpleName("ValueType").Qualify("System"))[0]); // make it a struct
        }
        else if (st.Name == CodeSymbols.Interface)
        {
            type.AddAttribute(FlagAttribute.InterfaceType);
        }

        ConversionUtils.SetAccessModifier(st, type);
        SetOtherModifiers(st, type);

        if (scope.Add(new TypeScopeItem { Name = name.Name, TypeInfo = type, SubScope = scope.CreateChildScope() }))
        {
            context.Assembly.AddType(type);
        }
        else
        {
            context.AddError(st, $"Type {name.Name} is already defined.");
        }
    }

    private static void SetOtherModifiers(LNode node, DescribedType type)
    {
        if (node.Attrs.Contains(LNode.Id(CodeSymbols.Static)))
        {
            type.IsStatic = true;
        }

        if (node.Attrs.Contains(LNode.Id(CodeSymbols.Abstract)))
        {
            type.IsAbstract = true;
        }
    }

    private void AddTypeAlias(LNode st, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        var name = st[0].Name.Name;
        var typeName = st[1];
        var type = TypeInheritanceStage.ResolveTypeWithModule(typeName, context, modulename);

        scope.TypeAliases.Add(name, type);
    }

    private IEnumerable<LNode> GetNamespaceImports(CompilationUnit cu, CompilerContext context)
    {
        for (var i = 0; i < cu.Body.Count; i++)
        {
            var node = cu.Body[i];

            if (i > 0 && !cu.Body[i - 1].Calls(CodeSymbols.Import) && node.Calls(CodeSymbols.Import))
            {
                context.AddError(node, "Imports are only allowed at the top of the file");
            }

            if (node.Calls(CodeSymbols.Import))
            {
                yield return node;
            }
        }
    }

    private void ConvertUnitDeclaration(CompilerContext context, LNode st, QualifiedName modulename, Scope globalScope)
    {
        var unitType = new DescribedType(
            new SimpleName(st[0].Name.ToString()).Qualify(modulename), context.Assembly)
        {
            IsStatic = true, IsPublic = true
        };

        unitType.AddAttribute(new DescribedAttribute(Utils.ResolveType(context.Binder, typeof(UnitTypeAttribute))));

        context.Assembly.AddType(unitType);
    }

    private void ConvertDiscriminatedUnion(CompilerContext context, LNode discrim, QualifiedName modulename)
    {
        var name = discrim.Args[0].Name;

        var baseType = new DescribedType(new SimpleName(name.Name).Qualify(modulename), context.Assembly);
        ConversionUtils.SetAccessModifier(discrim, baseType);
        baseType.IsAbstract = true;

        context.Assembly.AddType(baseType);

        foreach (var type in discrim.Args[1].Args)
        {
            var discName = type.Args[0].Name;
            var discType = new DescribedType(new SimpleName(discName.Name).Qualify(modulename), context.Assembly);
            ConversionUtils.SetAccessModifier(discrim, discType);
            discType.AddBaseType(baseType);
            context.Assembly.AddType(discType);

            foreach (var field in type.Args[1].Args)
            {
                var fieldName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                    field.Args[1].Args[0].Name.Name);
                var fieldTypename = ConversionUtils.GetQualifiedName(field.Args[0].Args[0].Args[0]);
                var fieldActualType = TypeInheritanceStage.ResolveTypeWithModule(
                    field.Args[0].Args[0].Args[0], context,
                    modulename, fieldTypename
                );
                if (baseType.Name.ToString() == fieldTypename.ToString())
                {
                    fieldActualType = baseType;
                }

                var fieldType = new DescribedField(discType, new SimpleName(fieldName),
                    false,
                    fieldActualType
                );

                if (field.Attrs.Any(_ => _.Name == Symbols.Mutable))
                {
                    fieldType.AddAttribute(Attributes.Mutable);
                }

                fieldType.IsPublic = true;
                discType.AddField(fieldType);
            }

            IRGenerator.GenerateGetHashCode(context, discType);
            IRGenerator.GenerateDefaultCtor(context, discType);
            IRGenerator.GenerateEmptyCtor(context, discType);
            IRGenerator.GenerateToString(context, discType);
        }
    }
}