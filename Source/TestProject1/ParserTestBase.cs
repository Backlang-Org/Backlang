using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject1
{
    public class ParserTestBase
    {
        protected static T ParseAndGetNode<T>(string source)
        {
            var ast = CompilationUnit.FromText(source);

            var node = ast.Body.Body.OfType<T>().FirstOrDefault();

            Assert.IsNotNull(node);
            Assert.AreEqual(ast.Messages.Count, 0);

            return node;
        }

        protected static T ParseAndGetNodeInFunction<T>(string source)
        {
            var tree = ParseAndGetNode<FunctionDeclaration>("fn main() {" + source + "}");

            return tree.Body.Body.OfType<T>().FirstOrDefault();
        }
    }
}