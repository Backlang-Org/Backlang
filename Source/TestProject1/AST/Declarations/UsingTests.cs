using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class UsingTests : ParserTestBase
{
    [TestMethod]
    public void IntUsing_Should_Pass()
    {
        var src = "using i32 as int;";
        var node = ParseAndGetNodes(src);
    }
}
