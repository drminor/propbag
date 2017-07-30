using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace DRM.PropBagModel
{
    public class PropModelReader_OLD
    {
        public List<PropItem> Contents { get; private set; }

        public PropModelReader_OLD()
        {
            Contents = new List<PropItem>();
        }

        public void ReadText(string path)
        {
            string[] lines = File.ReadAllLines(path);

            foreach (string l in lines)
            {
                if (l.Trim().Length < 2)
                {
                    continue;
                }
                string[] parts = l.Split(':');

                string n = parts[0];
                string t = parts[1];

                Contents.Add(new PropItem(n,t));
            }


        }
    }
}
