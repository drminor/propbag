using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

using System.Xml;

using DRM.Ipnwvc;
using DRM.PropBag;


namespace DRM.PropBagModel
{
    public class PropModelReader
    {

        static public PropModel ReadXml(string path)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(PropModel));

            using(FileStream fs = new FileStream(path, FileMode.Open))
            {
                //XmlDocument xx = new XmlDocument();
                //xx.Load(fs);

                return (PropModel)mySerializer.Deserialize(fs);
            }
        }

        static public string GetClassNameFromTemplateFilename(string n)
        {
            int pos = n.LastIndexOf('.');
            if (pos > 0)
            {
                n = n.Substring(0, pos);
                pos = n.LastIndexOf('.');
                if(pos > 0)
                    return n.Substring(0, pos);
                return n;
            }
            else
            {
                return n;
            }
        }

        static public string GetDefFileName(string n)
        {
            string t = string.Format("{0}_PropDefs.xml", n);
            return t;
        }

        static public void Test()
        {
            string path = @"C:\DEV\VS2013Projects\PubPropBag\PropBagT4\SampleT4Output.xml";

            PropModel pm = new PropModel("TestClass", "PropBagLib.Tests", PropBagTypeSafetyMode.Loose);

            pm.Props = new PropItem[3];

            pm.Props[0] = new PropItem("Prop1", "int");
            pm.Props[1] = new PropItem("Prop2", "string");
            pm.Props[2] = new PropItem("Prop3", "double");


            XmlSerializer mySerializer = new XmlSerializer(typeof(PropModel));

            // To read the file, create a FileStream.
            FileStream myFileStream = new FileStream(path, FileMode.OpenOrCreate);

            mySerializer.Serialize(myFileStream, pm);
        }
    }
}
