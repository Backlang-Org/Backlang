using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class ExpressionTests : ParserTestBase
{
    [TestMethod]
    public void ArrayAccess_With_Indices_Should_Pass()
    {
        var src = "arr[0, 1];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void ArrayAccess_With_Indices_With_Trailling_Comma_Should_Fail()
    {
        var src = "arr[0, 1,];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(1, result.errors.Count);
    }

    [TestMethod]
    public void InitializerList_With_Trailling_Comma_Should_Fail()
    {
        var src = "[0, 1,];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(1, result.errors.Count);
    }

    [TestMethod]
    public void ArrayAccess_With_One_Index_Addition_Should_Pass()
    {
        var src = "arr[1+2];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void ArrayAccess_With_One_Index_Should_Pass()
    {
        var src = "arr[0];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Empty_InitializerList_Should_Pass()
    {
        var src = "[];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void InitializerList_With_Element_Should_Pass()
    {
        var src = "[12];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void InitializerList_With_Elements_Should_Pass()
    {
        var src = "[1, 2];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void InitializerList_With_InitialierList_Element_Should_Pass()
    {
        var src = "[[]];";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void None_Should_Pass()
    {
        var src = "none;";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void SizeOf_Should_Pass()
    {
        var src = "sizeof(i32);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }
}