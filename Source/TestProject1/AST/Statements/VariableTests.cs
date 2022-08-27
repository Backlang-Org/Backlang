using Backlang.Codeanalysis.Parsing.AST;
using Loyc;
using Loyc.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Statements
{
    [TestClass]
    public class VariableTests : ParserTestBase
    {
        [TestMethod]
        public void VariableDeclaration_Full_MissMatch_Types_Should_Pass()
        {
            var src = "let hello : bool = \"true\";";

            var result = ParseAndGetNodesInFunction(src);
            var statement = result.nodes[0];
            var right = statement.Args[1];

            Assert.AreEqual(0, result.errors.Count);

            Assert.IsTrue(statement.Calls(CodeSymbols.Var));
            Assert.AreEqual((Symbol)"hello", right.Args[0].Name);
            Assert.AreEqual("true", right.Args[1].Args[0].Value);
        }

        [TestMethod]
        public void VariableDeclaration_Mutable_Full_BoolValue_Should_Pass()
        {
            var src = "let mut hello : bool = true;";

            var result = ParseAndGetNodesInFunction(src);
            var statement = result.nodes[0];
            var right = statement.Args[1];

            Assert.AreEqual(0, result.errors.Count);

            Assert.IsTrue(statement.Calls(CodeSymbols.Var));
            Assert.AreEqual((Symbol)"hello", right.Args[0].Name);
            Assert.AreEqual(LNode.Id(Symbols.Mutable), statement.Attrs[0]);
            Assert.AreEqual(true, right.Args[1].Args[0].Value);
        }

        [TestMethod]
        public void VariableDeclaration_Without_Type_Should_Pass()
        {
            var src = "let hello = 42;";

            var result = ParseAndGetNodesInFunction(src);
            var statement = result.nodes[0];

            var right = statement.Args[1];

            Assert.AreEqual(0, result.errors.Count);

            Assert.IsTrue(statement.Calls(CodeSymbols.Var));
            Assert.AreEqual((Symbol)"hello", right.Args[0].Name);
            Assert.AreEqual(42, right.Args[1].Args[0].Value);
        }

        [TestMethod]
        public void VariableDeclarationWithHex_Should_Pass()
        {
            var src = "let hello = 0xc0ffee;";
            var result = ParseAndGetNodesInFunction(src);
            var statement = result.nodes[0];

            var right = statement.Args[1];

            Assert.AreEqual(0, result.errors.Count);

            Assert.IsTrue(statement.Calls(CodeSymbols.Var));
            Assert.AreEqual((Symbol)"hello", right.Args[0].Name);
            Assert.AreEqual(0xC0ffee, right.Args[1].Args[0].Value);
        }
    }
}