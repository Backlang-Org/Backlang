using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;
using Backlang_Compiler.Compiling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace TestProject1;

[TestClass]
public class EmitterTests
{
    [TestMethod]
    public void EmitAddTerminalOutput_Should_Pass()
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize("{ mov 65, A; mov 1, B; add A, A, B; }");
        var parser = new Parser(null, tokens, lexer.Messages);

        var node = AssemblerBlockStatement.Parse(parser.Iterator, parser);
        var emitter = new AssemblyEmitter();
        var body = emitter.Emit((AssemblerBlockStatement)node);

        File.WriteAllBytes("emitter.backseat", body);
    }

    [TestMethod]
    public void EmitTerminalOutput_Should_Pass()
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize("{ mov 65, A; mov &[2], A; }");
        var parser = new Parser(null, tokens, lexer.Messages);

        var node = AssemblerBlockStatement.Parse(parser.Iterator, parser);
        var emitter = new AssemblyEmitter();
        var body = emitter.Emit((AssemblerBlockStatement)node);

        File.WriteAllBytes("emitter.backseat", body);
    }
}