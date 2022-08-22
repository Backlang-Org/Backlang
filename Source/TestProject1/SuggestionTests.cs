using Backlang.Codeanalysis.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class SuggestionTests
{
    [TestMethod]
    public void Let()
    {
        var src = new[] { "string", "i32", "let" };
        var test = "lt";
        var suggestion = LevensteinDistance.Suggest(test, src);

        Assert.AreEqual("let", suggestion);
    }

    [TestMethod]
    public void Similarity()
    {
        var src = new[] { "string", "i32", "i64", "let" };
        var test = "i6";
        var suggestion = LevensteinDistance.Suggest(test, src);

        Assert.AreEqual("i64", suggestion);
    }
}