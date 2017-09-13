using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Inpcwv;
using DRM.PropBag;

using NUnit.Framework;
using System.Dynamic;

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

        [Test]
        public void TestDynamicTypeSafety()
        {
            dynamic td = new PropBagDyn();

            PropBagDyn pbd = td as PropBagDyn;

            pbd.AddProp<string>("MyString", null, false, null, null, "hello");

            td.MyString = "Good Bye";
    

        }

        #endregion



    }
}
