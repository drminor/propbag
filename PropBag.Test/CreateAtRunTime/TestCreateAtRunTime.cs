using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using DRM.PropBag;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestCreateAtRunTime
    {
        public CreateAtRunTimeModel mod1;

        [SetUp]
        public void Create()
        {
        }

        [TearDown]
        public void destroy()
        {
            mod1 = null;
        }


        [Test]
        public void Test1()
        {
            mod1 = new CreateAtRunTimeModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);


        }

    }
}
