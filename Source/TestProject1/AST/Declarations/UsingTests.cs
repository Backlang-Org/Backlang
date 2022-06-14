using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class UsingTests : ParserTestBase
{
    [TestMethod]
    public void IntUsing_Should_Pass()
    {
        var src = "using int as i32;";
        var node = ParseAndGetNodes(src);
    }
}
