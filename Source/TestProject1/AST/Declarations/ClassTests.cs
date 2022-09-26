using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class ClassTests : ParserTestBase
{
    [TestMethod]
    public void Simple_Class_Should_Pass()
    {
        var src = "class Point { let X : i32; let Y : i32; }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Class_With_Inheritance_Should_Pass()
    {
        var src = "class Point : IHello, IWorld { let X : i32; let Y : i32; }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }
}