﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class ImplementationTests : ParserTestBase
{
    [TestMethod]
    public void Range_Impl_Should_Pass()
    {
        var src = "implement u8..u32 { func something() {  } }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Range_Half_Impl_Should_Fail()
    {
        var src = "implement u8.. { func something() {  } }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(1, result.errors.Count);
    }

    [TestMethod]
    public void Simple_Impl_Should_Pass()
    {
        var src = "implement u8 { func something() {  } }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Impl_Function_With_Annotations_Should_Pass()
    {
        var src = "implement u8 { @Log func something() {  } }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Impl_Function_With_Modifiers_Should_Pass()
    {
        var src = "implement u8 { private override func something() {  } }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }
}