using DRM.PropBag.ClassGenerator;
using DRM.PropBag.XMLModel;
using DRM.TypeSafePropertyBag; using Swhp.Tspb.PropBagAutoMapperService;
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

            XMLPropModel pm = PropModelReader.ReadXml(propDefsPath);

            Assert.That(pm, Is.Not.EqualTo(null), "PropModelReader returned null");

            string nameSpaceText = T4Support.GetNamespaces(pm);

            List<XMLPropItemModel> test = pm.Props;

            Assert.That(pm.Props.Count(), Is.EqualTo(11));
        }

        [Test]
        public void WriteXml()
        {
            XMLPropModel pm = new XMLPropModel
            {
                ClassName = "TestOu",
                DeriveFromPubPropBag = false,
                DeferMethodRefResolution = false,
                Namespace = "PropBagLib.Tests",
                Props = new List<XMLPropItemModel>()
            };

            XMLPropItemModel p = new XMLPropItemModel
            {
                Name = "one",
                Type = "int",
                InitialValueField = new PropInitialValueField("1"),
                StorageStrategy = PropStorageStrategyEnum.Internal
            };

            pm.Props.Add(p);

            p = new XMLPropItemModel
            {
                Name = "two",
                Type = "string",
                InitialValueField = new PropInitialValueField("1"),
                StorageStrategy = PropStorageStrategyEnum.Internal
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
