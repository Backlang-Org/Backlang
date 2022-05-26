using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class CallExpressionTests : ParserTestBase
{
    [TestMethod]
    public void Call_With_Array_Index_Argument_Should_Pass()
    {
        var src = "print(arr[1]);";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void Call_With_One_Argument_Should_Pass()
    {
        var src = "print(true);";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void Call_With_Two_Arguments_Should_Pass()
    {
        var src = "print(1, true);";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void Call_Without_Arguments_Should_Pass()
    {
        var src = "print();";
        var tree = ParseAndGetNodesInFunction(src);
    }
}