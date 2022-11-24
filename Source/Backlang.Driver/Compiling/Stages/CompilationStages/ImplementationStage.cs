using Backlang.Codeanalysis.Core;
using Backlang.Contracts.Scoping.Items;
using Flo;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public sealed partial class ImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var modulename = ConversionUtils.GetModuleName(tree);

            foreach (var node in tree.Body)
            {
                CollectImplementations(context, node, modulename);
                ImplementDefaultsForStructs(context, node, modulename);
            }

            ConvertMethodBodies(context);
        }

        return await next.Invoke(context);
    }

    private static void ImplementDefaultsForStructs(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == CodeSymbols.Struct)) return;

        var name = st.Args[0].Name;
        var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(modulename)).FirstOrDefault();

        // toString method
        if (!type.Methods.Any(_ => _.Name.ToString() == "ToString" && _.Parameters.Count == 0))
        {
            IRGenerator.GenerateToString(context, type);
        }

        // default constructor
        if (!type.Methods.Any(_ => _.Name.ToString() == "new" && _.Parameters.Count == type.Fields.Count))
        {
            IRGenerator.GenerateDefaultCtor(context, type);
        }

        IRGenerator.GenerateEmptyCtor(context, type);

        if (!type.Methods.Any(_ => _.Name.ToString() == "GetHashCode" && _.Parameters.Count == 0))
        {
            IRGenerator.GenerateGetHashCode(context, type);
        }
    }

    private static void CollectImplementations(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == Symbols.Implementation)) return;

        var typenode = st.Args[0].Args[0].Args[0].Args[0];
        var fullname = ConversionUtils.GetQualifiedName(typenode);

        DescribedType targetType = null;
        Scope typeScope = null;
        if (context.GlobalScope.TryGet<TypeScopeItem>(fullname.FullName.ToString(), out var typeItem))
        {
            targetType = (DescribedType)typeItem.Type;
            typeItem.Deconstruct(out _, out _, out typeScope, out _);
        }
        else
        {
            targetType = (DescribedType)TypeInheritanceStage.ResolveTypeWithModule(typenode, context, modulename, fullname);

            if (targetType == null)
            {
                context.AddError(typenode, new(ErrorID.CannotImplementTypeNotFound, fullname.FullName));
                return;
            }

            if (Utils.IsUnitType(context, targetType))
            {
                context.AddError(typenode, new(ErrorID.CannotImplementUnitType, fullname.FullName));
            }

            typeScope = context.GlobalScope.CreateChildScope();
        }

        var body = st.Args[0].Args[1].Args;

        foreach (var node in body)
        {
            if (node.Name == CodeSymbols.Fn)
            {
                if (targetType.Parent.Assembly == context.Assembly)
                {
                    var fn = TypeInheritanceStage.ConvertFunction(context, targetType, node, modulename, typeScope);
                    targetType.AddMethod(fn);
                }
                else
                {
                    var extensionType = (DescribedType)context.Binder.ResolveTypes(new SimpleName(Names.Extensions).Qualify(modulename)).FirstOrDefault();

                    if (extensionType == null)
                    {
                        extensionType = new DescribedType(new SimpleName(Names.Extensions).Qualify(modulename), context.Assembly)
                        {
                            IsStatic = true,
                            IsPublic = true
                        };
                        context.Assembly.AddType(extensionType);
                    }

                    var fn = TypeInheritanceStage.ConvertFunction(context, extensionType, node, modulename, typeScope);

                    fn.IsStatic = true;

                    var param = (IList<Parameter>)fn.Parameters;

                    param.Insert(0, Parameter.CreateThisParameter(targetType));

                    var extType = Utils.ResolveType(context.Binder, typeof(ExtensionAttribute));

                    fn.AddAttribute(new DescribedAttribute(extType));

                    extensionType.AddMethod(fn);
                }
            }
        }
    }
}