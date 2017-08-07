using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using NUnit.Framework;

using DRM.PropBagModel;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestT4
    {

        [Test]
        public void ReadInput()
        {
            string PropDefsFilename = "PropGen_PropDefs.xml";
            string excPath = System.AppDomain.CurrentDomain.BaseDirectory;

            string projectFolderPath = FileUtils.GetProjectFolder(excPath);

            string propDefsPath = System.IO.Path.Combine(projectFolderPath, "T4", PropDefsFilename);

            PropModel pm = PropModelReader.ReadXml(propDefsPath);

            Assert.That(pm, Is.Not.EqualTo(null), "PropModelReader returned null");

            string nameSpaceText = pm.GetNamespaces();

            foreach (PropItem pi in pm.Props)
            {
                string AddPropText = pm.GetAddPropMethodCallText(pi);
            }

        }

        [Test]
        public void RunPropGenTemplate()
        {

        }
    }
}
