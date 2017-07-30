using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using DRM.PropBagModel;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestT4
    {

        const string InputPath = @"C:\DEV\VS2013Projects\PubPropBag\PropBagT4\TestInput.xml";

        [Test]
        public void ReadInput()
        {
            PropModel pm = PropModelReader.ReadXml(InputPath);

            Assert.That(pm, Is.Not.EqualTo(null), "PropModelReader returned null");
            //PropModelReaderX.Test();
        }

        [Test]
        public void RunPropGenTemplate()
        {

        }
    }
}
