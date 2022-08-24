using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Loyc.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace TestProject1
{
    public class ParserTestBase
    {
        protected static (LNodeList nodes, List<Message> errors) ParseAndGetNodes(string source)
        {
            var ast = CompilationUnit.FromText(source);

            var node = ast.Body;

            Assert.IsNotNull(node);

            return (node, ast.Messages);
        }

        protected static (LNode nodes, List<Message> errors) ParseAndGetNode(string source)
        {
            var result = ParseAndGetNodes(source);

            return (result.nodes.First(), result.errors);
        }

        protected static (LNodeList nodes, List<Message> errors) ParseAndGetNodesInFunction(string source)
        {
            var tree = ParseAndGetNodes("func main() {" + source + "}");

            return (tree.Item1.First().Args[3].Args, tree.Item2);
        }
    }
}