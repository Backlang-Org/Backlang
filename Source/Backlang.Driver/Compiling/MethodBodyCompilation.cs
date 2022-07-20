using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling;
public readonly record struct MethodBodyCompilation(LNode function, CompilerContext context, DescribedBodyMethod method, QualifiedName? modulename);