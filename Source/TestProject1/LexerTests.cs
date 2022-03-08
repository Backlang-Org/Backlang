using Backlang.Codeanalysis.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void Lexer_HexNumber_Should_Pass()
        {
            var src = "0xc0ffee";
            var lexer = new Lexer();
            var tokens = lexer.Tokenize(src);

            Assert.AreEqual(tokens.Count, 2);
            Assert.AreEqual(tokens[0].Type, TokenType.HexNumber);
            Assert.AreEqual(tokens[0].Text, "0xc0ffee");
        }

        [TestMethod]
        public void Lexer_IdentifierWithUnderscore_Should_Pass()
        {
            var src = "hello_world";
            var lexer = new Lexer();
            var tokens = lexer.Tokenize(src);

            Assert.AreEqual(tokens.Count, 2);
            Assert.AreEqual(tokens[0].Type, TokenType.Identifier);
            Assert.AreEqual(tokens[0].Text, "hello_world");
        }

        [TestMethod]
        public void Lexer_Multiple_Char_Symbol_Should_Pass()
        {
            var src = "a <-> b";
            var lexer = new Lexer();
            var tokens = lexer.Tokenize(src);

            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual(tokens[1].Type, TokenType.SwapOperator);
        }
    }
}