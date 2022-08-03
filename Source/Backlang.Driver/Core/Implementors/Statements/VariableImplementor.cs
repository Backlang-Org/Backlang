using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Contracts.Scoping.Items;
using Backlang.Driver.Compiling.Stages;
using Backlang.Driver.Compiling.Stages.CompilationStages;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Loyc.Syntax;

namespace Backlang.Driver.Core.Implementors.Statements;

public class VariableImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        var decl = node.Args[1];

        var name = ConversionUtils.GetQualifiedName(node.Args[0]);
        var elementType = TypeInheritanceStage.ResolveTypeWithModule(node.Args[0], context, modulename.Value, name);

        var varname = decl.Args[0].Name.Name;
        var isMutable = node.Attrs.Contains(LNode.Id(Symbols.Mutable));

        if (scope.Add(new VariableScopeItem { Name = varname, IsMutable = isMutable }))
        {
            block.AppendParameter(new BlockParameter(elementType, varname));
        }
        else
        {
            context.AddError(decl.Args[0], $"{varname} already declared");
        }

        ImplementationStage.AppendExpression(block, decl.Args[1], elementType, method);

        block.AppendInstruction(Instruction.CreateAlloca(elementType));

        return block;
    }
}