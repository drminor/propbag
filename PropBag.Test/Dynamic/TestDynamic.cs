using DRM.PropBag; using DRM.TypeSafePropertyBag;

using NUnit.Framework;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestDynamic
    {
        #region Setup and TearDown

        [OneTimeSetUp]
        public void Create()
        {
        }

        [OneTimeTearDown]
        public void Destroy()
        {
        }

        #endregion

        #region Tests

        //[Test]
        //public void TestDynamicTypeSafety()
        //{
        //    dynamic td = new PropBagDyn();

        //    PropBagDyn pbd = td as PropBagDyn;

        //    pbd.AddProp<string>("MyString", null, false, null, null, "hello");

        //    td.MyString = "Good Bye";

        //    Assert.That(td.MyString == "Good Bye");
    

        //}

        #endregion



    }
}
