using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Driver.Compiling.Stages.CompilationStages;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Driver.Core.Implementors.Statements;

public class StaticCallImplementor : IStatementImplementor, IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.ColonColon);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block, IType elementType, IMethod method, CompilerContext context)
    {
        var callee = node.Args[1];
        var typename = ConversionUtils.GetQualifiedName(node.Args[0]);

        var type = (DescribedType)context.Binder.ResolveTypes(typename).FirstOrDefault();

        return ImplementationStage.AppendCall(context, block, callee, type.Methods, callee.Name.Name);
    }

    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        Handle(node, block, TypeDeducer.Deduce(node, scope, context), method, context);

        return block;
    }
}