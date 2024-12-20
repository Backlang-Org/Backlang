﻿using Backlang.Codeanalysis.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class LexerTests
{
    [TestMethod]
    public void Lexer_Arrow_Should_Pass()
    {
        var src = "a -> b";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(new SourceDocument("test", src));

        Assert.AreEqual(4, tokens.Count);
        Assert.AreEqual(tokens[1].Type, TokenType.Arrow);
    }

    [TestMethod]
    public void Lexer_BinNumber_Should_Pass()
    {
        var src = "0x1011011110110110";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(new SourceDocument("test", src));

        Assert.AreEqual(tokens.Count, 2);
        Assert.AreEqual(tokens[0].Type, TokenType.HexNumber);
        Assert.AreEqual(tokens[0].Text, "1011011110110110");
    }

    [TestMethod]
    public void Lexer_BinNumber_With_Seperator_Should_Pass()
    {
        var src = "0x1011_0111_1011_0110";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(new SourceDocument("test", src));

        Assert.AreEqual(tokens.Count, 2);
        Assert.AreEqual(tokens[0].Type, TokenType.HexNumber);
        Assert.AreEqual(tokens[0].Text, "1011011110110110");
    }

    [TestMethod]
    public void Lexer_HexNumber_Should_Pass()
    {
        var src = "0xc0ffee";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(new SourceDocument("test", src));

        Assert.AreEqual(tokens.Count, 2);
        Assert.AreEqual(tokens[0].Type, TokenType.HexNumber);
        Assert.AreEqual(tokens[0].Text, "c0ffee");
    }

    [TestMethod]
    public void Lexer_HexNumber_With_Seperator_Should_Pass()
    {
        var src = "0xc0_ff_ee";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(new SourceDocument("test", src));

        Assert.AreEqual(tokens.Count, 2);
        Assert.AreEqual(tokens[0].Type, TokenType.HexNumber);
        Assert.AreEqual(tokens[0].Text, "c0ffee");
    }

    [TestMethod]
    public void Lexer_IdentifierWithUnderscore_Should_Pass()
    {
        var src = "hello_world";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(new SourceDocument("test", src));

        Assert.AreEqual(tokens.Count, 2);
        Assert.AreEqual(tokens[0].Type, TokenType.Identifier);
        Assert.AreEqual(tokens[0].Text, "hello_world");
    }

    [TestMethod]
    public void Lexer_Multiple_Char_Symbol_Should_Pass()
    {
        var src = "a <-> b";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(new SourceDocument("test", src));

        Assert.AreEqual(4, tokens.Count);
        Assert.AreEqual(tokens[1].Type, TokenType.SwapOperator);
    }
}