using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class VariableTests : ParserTestBase
    {
        [TestMethod]
        public void VariableAssignment_Multiple_Should_Pass()
        {
            var src = "a = b = c = 5;";
            var statement = ParseAndGetNode<ExpressionStatement>(src);
            var expr = (BinaryExpression)statement.Expression;

            Assert.IsInstanceOfType(expr.Right, typeof(LiteralNode));
        }

        [TestMethod]
        public void VariableAssignment_Should_Pass()
        {
            var src = "hello = 42;";
            var statement = ParseAndGetNode<ExpressionStatement>(src);
            var expr = (BinaryExpression)statement.Expression;

            Assert.AreEqual(expr.OperatorToken.Text, "=");
            Assert.IsInstanceOfType(expr.Left, typeof(NameExpression));

            Assert.AreEqual(((NameExpression)expr.Left).Name, "hello");
            Assert.AreEqual(((LiteralNode)expr.Right).Value, 42);
        }

        [TestMethod]
        public void VariableDeclaration_Full_BoolValue_Should_Pass()
        {
            var src = "declare hello : bool = true;";

            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(statement.TypeToken.Text, "bool");

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, true);
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
            Assert.AreEqual(statement.TypeToken.Text, "bool");

            Assert.IsInstanceOfType(statement.Value, typeof(LiteralNode));
            Assert.AreEqual(((LiteralNode)statement.Value).Value, "true");
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

        [TestMethod]
        public void VariableDeclarationWithBinary_Should_Pass()
        {
            var src = "declare hello = 0b10101;";
            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 0b10101);
        }

        [TestMethod]
        public void VariableDeclarationWithHex_Should_Pass()
        {
            var src = "declare hello = 0xc0ffee;";
            var statement = ParseAndGetNode<VariableDeclarationStatement>(src);

            Assert.AreEqual(statement.NameToken.Text, "hello");
            Assert.AreEqual(((LiteralNode)statement.Value).Value, 0xc0ffee);
        }
    }
}