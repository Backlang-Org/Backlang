using Backlang_Compiler.Parsing;
using Backlang_Compiler.Parsing.AST;
using Backlang_Compiler.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
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

        [TestMethod]
        public void VariableDeclarationFull_Should_Pass()
        {
            var src = "declare hello : i32 = 42;";
            var ast = CompilationUnit.FromText(src);

            var statement = ast.Body.Body.OfType<VariableDeclarationStatement>().FirstOrDefault();

            Assert.IsNotNull(statement);
            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(statement.TypeToken.Text, "i32");

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 42);
        }
    }
}