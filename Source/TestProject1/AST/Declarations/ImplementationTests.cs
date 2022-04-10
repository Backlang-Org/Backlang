using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class ImplementationTests : ParserTestBase
{
    [TestMethod]
    public void Range_Impl_Should_Pass()
    {
        var src = "implementation of u8..u32 { fn something() {  } }";
        var node = ParseAndGetNode<ImplementationDeclaration>(src);
    }

    [TestMethod]
    public void Simple_Impl_Should_Pass()
    {
        var src = "implementation of u8 { fn something() {  } }";
        var node = ParseAndGetNode<ImplementationDeclaration>(src);
    }
}