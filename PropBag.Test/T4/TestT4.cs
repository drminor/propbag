using DRM.PropBag.ClassGenerator;
using DRM.PropBag.XMLModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace PropBagLib.Tests
{
    [TestFixture]
    public class TestT4
    {
        [Test]
        public void ReadInput()
        {
            string PropDefsFilename = "Sample_PropDefs.xml";
            string excPath = System.AppDomain.CurrentDomain.BaseDirectory;

            string projectFolderPath = FileUtils.GetProjectFolder(excPath);

            string propDefsPath = System.IO.Path.Combine(projectFolderPath, "T4", PropDefsFilename);

            PropModel pm = PropModelReader.ReadXml(propDefsPath);

            Assert.That(pm, Is.Not.EqualTo(null), "PropModelReader returned null");

            string nameSpaceText = T4Support.GetNamespaces(pm);

            List<PropItem> test = pm.Props;

            Assert.That(pm.Props.Count(), Is.EqualTo(11));
        }

        [Test]
        public void WriteXml()
        {
            PropModel pm = new PropModel
            {
                ClassName = "TestOu",
                DeriveFromPubPropBag = false,
                DeferMethodRefResolution = false,
                Namespace = "PropBagLib.Tests",
                Props = new List<PropItem>()
            };

            PropItem p = new PropItem
            {
                Name = "one",
                Type = "int",
                InitalValueField = new PropInitialValueField("1"),
                HasStore = true
            };

            pm.Props.Add(p);

            p = new PropItem
            {
                Name = "two",
                Type = "string",
                InitalValueField = new PropInitialValueField("1"),
                HasStore = true
            };

            pm.Props.Add(p);

            string outFileName = "TestSerialization.xml";
            string excPath = System.AppDomain.CurrentDomain.BaseDirectory;

            string projectFolderPath = FileUtils.GetProjectFolder(excPath);

            string outPath = System.IO.Path.Combine(projectFolderPath, "T4", outFileName);


            PropModelWriter.WriteXml(outPath, pm);


        }
    }
}
