using Lljxww.Utilities.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lljxww.Test;

[TestClass]
public class CryptographyUnitTest
{
    [TestMethod]
    public void TestMethod1()
    {
        string source = "liangjw";
        string target = "bc9a767ffd41098041cdec50424a5f4dae9c23b0";

        Assert.AreEqual(target, SHA1.Calculate(source));
    }
}