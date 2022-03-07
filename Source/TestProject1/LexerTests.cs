using Microsoft.VisualStudio.TestTools.UnitTesting;
using Backlang.Codeanalysis.Parsing;

namespace TestProject1
{
    [TestClass]
    public class LexerTests
    {
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