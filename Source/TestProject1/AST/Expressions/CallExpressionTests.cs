using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class CallExpressionTests : ParserTestBase
{
    [TestMethod]
    public void Call_With_Array_Index_Argument_Should_Pass()
    {
        var src = "print(arr[1]);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Call_With_One_Argument_Should_Pass()
    {
        var src = "print(true);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Call_With_One_Argument_With_Trailling_Comma_Should_Fail()
    {
        var src = "print(true,);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(1, result.errors.Count);
    }

    [TestMethod]
    public void Call_With_Two_Arguments_Should_Pass()
    {
        var src = "print(1, true);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Call_Without_Arguments_Should_Pass()
    {
        var src = "print();";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }
}