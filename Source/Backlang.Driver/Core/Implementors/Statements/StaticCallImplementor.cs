using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class StaticCallImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        var callee = node.Args[1];
        var typename = ConversionUtils.GetQualifiedName(node.Args[0]);

        var type = (DescribedType)context.Binder.ResolveTypes(typename).FirstOrDefault();

        AppendCall(context, block, callee, type.Methods, callee.Name.Name);

        return block;
    }
}