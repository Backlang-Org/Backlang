using Backlang_Compiler.Parsing.AST;
using Backlang_Compiler.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class VariableTests : ParserTestBase
    {
        [TestMethod]
        public void VariableDeclaration_Full_BoolValue_Should_Pass()
        {
            var src = "declare hello : bool = true;";

            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(statement.TypeToken.Text, "i32");

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 42);
        }

        [TestMethod]
        public void VariableDeclaration_Full_IntValue_Should_Pass()
        {
            var src = "declare hello : i32 = 42;";

            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(statement.TypeToken.Text, "i32");

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 42);
        }

        [TestMethod]
        public void VariableDeclaration_Full_MissMatch_Types_Should_Pass()
        {
            var src = "declare hello : bool = 'true';";

            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(statement.TypeToken.Text, "i32");

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 42);
        }

        [TestMethod]
        public void VariableDeclaration_Without_Type_Should_Pass()
        {
            var src = "declare hello = 42;";

            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.IsNull(statement.TypeToken);

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 42);
        }

        [TestMethod]
        public void VariableDeclaration_Without_Value_Should_Pass()
        {
            var src = "declare hello : i32;";

            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(statement.TypeToken.Text, "i32");
            Assert.IsNull(statement.Value);
        }
    }
}