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
        public void VariableDeclaration_Full_Should_Pass()
        {
            var src = "declare hello : i32 = 42;";
            var ast = CompilationUnit.FromText(src);

            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(statement.TypeToken.Text, "i32");

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 42);
        }

        private static T ParseAndGetNode<T>(string source)
        {
            var src = "declare hello : i32 = 42;";
            var ast = CompilationUnit.FromText(src);

            var node = ast.Body.Body.OfType<T>().FirstOrDefault();

            Assert.IsNotNull(node);

            return node;
        }
    }
}