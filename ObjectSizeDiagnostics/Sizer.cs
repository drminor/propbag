using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSizeDiagnostics
{
    public class Sizer
    {
        #region GET SIZE

        static public long GetSizeGC(Func<Object> loader)
        {
            object target;

            long startBytes = System.GC.GetTotalMemory(true);

            // Load the target with the new object.
            target = loader();

            long stopBytes = System.GC.GetTotalMemory(true);

            long size = (long)(stopBytes - startBytes);

            //string result = "Size is " + ((long)(stopBytes - startBytes)).ToString();

            GC.KeepAlive(target); // This ensure a reference to object keeps object in memory

            return size;
        }

        static public long GetSizeBinSer(Func<Object> loader)
        {
            long size = 0;

            // Load the target with the new object.
            object target = loader();

            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, target);
                size = s.Length;
            }

            return size;
        }

        static public long ReportMemConsumption(long startBytes, long prevBytes, string message)
        {
            long curBytes = System.GC.GetTotalMemory(true);
            System.Diagnostics.Debug.WriteLine($"Used: {curBytes - prevBytes}; Total: {curBytes - startBytes}: {message}.");
            return curBytes;
        }

        #endregion

    }
}
