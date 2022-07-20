using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var modulename = Utils.GetModuleName(tree);

            foreach (var node in tree.Body)
            {
                CollectImplementations(context, node, modulename);
                ImplementDefaultConstructors(context, node, modulename);
            }
        }

        return await next.Invoke(context);
    }

    private static void ImplementDefaultConstructors(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == CodeSymbols.Struct)) return;

        var name = st.Args[0].Name;
        var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(modulename)).FirstOrDefault();

        if (!type.Methods.Any(_ => _.Name.ToString() == "new" && _.Parameters.Count == type.Fields.Count))
        {
            var ctorMethod = new DescribedBodyMethod(type, new SimpleName("new"), true, ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)))
            {
                IsConstructor = true
            };

            ctorMethod.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

            foreach (var field in type.Fields)
            {
                ctorMethod.AddParameter(new Parameter(field.FieldType, field.Name));
            }

            ctorMethod.Body = null; // ToDo: Make body set members (Lixou)

            type.AddMethod(ctorMethod);
        }
    }

    private static void CollectImplementations(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == Symbols.Implementation)) return;

        var typenode = st.Args[0].Args[0].Args[0].Args[0];
        var fullname = Utils.GetQualifiedName(typenode);
        var targetType = TypeInheritanceStage.ResolveTypeWithModule(typenode, context, modulename, fullname);

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