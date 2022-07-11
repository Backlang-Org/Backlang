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
            CollectImplementations(context, tree);
            ImplementDefaultConstructors(context, tree);
        }

        return await next.Invoke(context);
    }

    private void CollectImplementations(CompilerContext context, CompilationUnit tree)
    {
        foreach (var st in tree.Body)
        {
            if (!(st.IsCall && st.Name == Symbols.Implementation)) continue;

            var targetType = (DescribedType)IntermediateStage.GetType(st.Args[0], context);
            var body = st.Args[0].Args[1].Args;

            foreach (var node in body)
            {
                if (node.Name == CodeSymbols.Fn)
                {
                    if (targetType.Parent.Assembly == context.Assembly)
                    {
                        var fn = TypeInheritanceStage.ConvertFunction(context, targetType, node);
                        targetType.AddMethod(fn);
                    }
                    else
                    {
                        var fn = TypeInheritanceStage.ConvertFunction(context, context.ExtensionsType, node);

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

    private void ImplementDefaultConstructors(CompilerContext context, CompilationUnit tree)
    {
        foreach (var st in tree.Body)
        {
            if (!(st.IsCall && st.Name == CodeSymbols.Struct)) continue;

            var name = st.Args[0].Name;
            var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(context.Assembly.Name)).First();
            if (!type.Methods.Any(_ => _.Name.ToString() == "new" && _.Parameters.Count == type.Fields.Count))
            {
                var ctorMethod = new DescribedBodyMethod(type, new SimpleName("new"), true, ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)));
                ctorMethod.IsConstructor = true;
                ctorMethod.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

                foreach (var field in type.Fields)
                {
                    ctorMethod.AddParameter(new Parameter(field.FieldType, field.Name));
                }

                ctorMethod.Body = null; // ToDo: Make body set members

                type.AddMethod(ctorMethod);
            }
        }
    }
}