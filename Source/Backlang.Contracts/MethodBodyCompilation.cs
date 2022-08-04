using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Contracts;

public readonly record struct MethodBodyCompilation(LNode Function, CompilerContext Context,
    DescribedBodyMethod Method, QualifiedName? Modulename, Scope Scope);