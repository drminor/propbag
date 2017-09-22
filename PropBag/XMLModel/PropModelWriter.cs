using System;
using System.Collections.Generic;

using System.IO;
using System.Xml.Serialization;
using System.Xml;

using DRM.PropBag;

namespace DRM.PropBag.XMLModel
{
    public class PropModelWriter
    {

        static public void WriteXml(string path, PropModel pm)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(PropModel));

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                mySerializer.Serialize(fs, pm);
            }
        }
    }
}
