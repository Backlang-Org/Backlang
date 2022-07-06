using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;
using System.Runtime.InteropServices;

namespace Backlang.Driver.Compiling.Stages;

internal class LoweringStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            foreach (var node in tree.Body)
            {
                LowerUnion(node, context.Assembly, context);
            }
        }

        return await next(context);
    }

    private void LowerUnion(LNode node, DescribedAssembly ass, CompilerContext context)
    {
        if (node.Calls(Symbols.Union))
        {
            var type = new DescribedType(new SimpleName(node.Args[0].Name.Name).Qualify(ass.FullName.FullName), ass);
            type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("ValueType").Qualify("System")).First());

            var attributeType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(StructLayoutAttribute));

            var attribute = new DescribedAttribute(attributeType);

            type.AddAttribute(attribute);

            foreach (var member in node.Args[1].Args)
            {
                if (member.Name == CodeSymbols.Var)
                {
                    var mtype = IntermediateStage.GetType(member.Args[0], context);

                    var mvar = member.Args[1];
                    var mname = mvar.Args[0].Name;
                    var mvalue = mvar.Args[1];

                    var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

                    type.AddField(field);
                }
            }

            ass.AddType(type);
        }
    }
}