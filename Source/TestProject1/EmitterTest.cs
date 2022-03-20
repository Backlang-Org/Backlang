﻿using Backlang.Codeanalysis.Parsing;
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
        var tokens = lexer.Tokenize("{ mov A, 65; mov &[0], A; hlt;}");
        var parser = new Parser(null, tokens, lexer.Messages);

        var node = AssemblerBlockStatement.Parse(parser.Iterator, parser);
        var emitter = new AssemblyEmitter();
        var body = emitter.Emit((AssemblerBlockStatement)node, out var _);

        File.WriteAllBytes("emitter.backseat", body);
    }

    [TestMethod]
    public void EmitCounterTerminalWithJumpOutput_Should_Pass()
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize("{ mov 65, &[0]; mov &[0], A; add A, A, 1; mov A, &[0]; jmp 0x81F48 + 16; }");
        var parser = new Parser(null, tokens, lexer.Messages);

        var node = AssemblerBlockStatement.Parse(parser.Iterator, parser);
        var emitter = new AssemblyEmitter();

        var body = emitter.Emit((AssemblerBlockStatement)node, out var e);

        File.WriteAllBytes("emitter.backseat", body);
    }

    [TestMethod]
    public void EmitTerminalOutput_Should_Pass()
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize("{ mov 66, &[0]; mov 65, A; mov A, &[0x4]; mov 67, &[0x8]; hlt; }");
        var parser = new Parser(null, tokens, lexer.Messages);

        var node = AssemblerBlockStatement.Parse(parser.Iterator, parser);
        var emitter = new AssemblyEmitter();
        var body = emitter.Emit((AssemblerBlockStatement)node, out var _);

        File.WriteAllBytes("emitter.backseat", body);
    }

    [TestMethod]
    public void EmitTerminalWithJumpOutput_Should_Pass()
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize("{ mov 65, &[0]; mov &[0], A; add A, A, 1; add B, B, 0x4; mov B, &[PTR 0xFF]; jmp 0; hlt; }");
        var parser = new Parser(null, tokens, lexer.Messages);

        var node = AssemblerBlockStatement.Parse(parser.Iterator, parser);
        var emitter = new AssemblyEmitter();
        var body = emitter.Emit((AssemblerBlockStatement)node, out var _);

        File.WriteAllBytes("emitter.backseat", body);
    }
}