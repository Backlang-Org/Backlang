using Backlang.Contracts.ConstraintSystem;
using Loyc;
using Loyc.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class ConstraintTests
{
    [TestMethod]
    public void Value_Should_Pass()
    {
        var evaluator = new ConstraintEvaluator();
        var nodes = LNode.List(LNode.Call(CodeSymbols.Int32, LNode.List(LNode.Literal(42))));
        var result = evaluator.Evaluate(nodes, null);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Binary_Boolean_Should_Pass()
    {
        var evaluator = new ConstraintEvaluator();
        var nodes = LNode.List(LNode.Call(CodeSymbols.Int32, LNode.List(LNode.Literal(5))), LNode.Call(CodeSymbols.Int32, LNode.List(LNode.Literal(8))));
        var result = evaluator.Evaluate(LNode.List(LNode.Call(CodeSymbols.LE, nodes)), null);

        Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void Binary_Boolean_Is_Should_Pass()
    {
        var evaluator = new ConstraintEvaluator();
        evaluator.Variables.Add("x", 42);

        var nodes = LNode.List(LNode.Id("x"), LNode.Call(CodeSymbols.Int32, LNode.List(LNode.Literal(null))));
        var result = evaluator.Evaluate(LNode.List(LNode.Call(CodeSymbols.Is, nodes)), null);

        Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void Variables_Should_Pass()
    {
        var evaluator = new ConstraintEvaluator();
        evaluator.Variables.Add("x", 42);

        var result = evaluator.Evaluate(LNode.List(LNode.Id("x")), null);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Constraint_Call_Should_Pass()
    {
        var evaluator = new ConstraintEvaluator();
        evaluator.Variables.Add("x", 42);
        evaluator.Constraints.Add("do", LNode.Call((Symbol)"'==", LNode.List(LNode.Id("x"), LNode.Id("value"))));

        var result = evaluator.Evaluate(LNode.List(LNode.Call((Symbol)"#constraint", LNode.List(LNode.Id("do")))), 42);

        Assert.AreEqual(true, result);
    }
}