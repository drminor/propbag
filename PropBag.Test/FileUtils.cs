using System;
using System.IO;

namespace PropBagLib.Tests
{
    public class FileUtils
    {
        /// <summary>
        /// Used for projects being debugged withing Visual Studio.
        /// Returns the path to the project folder, given the path to the currently running executuble.
        /// The executable will be in either debug/bin or release/bin.
        /// </summary>
        /// <param name="assemblyBaseFolder">The path to the curent executable.</param>
        /// <returns>The grandparent folder of the given path for the Assembly's Base Folder.</returns>
        static public string GetProjectFolder(string assemblyBaseFolder)
        {
            if (assemblyBaseFolder == null || assemblyBaseFolder.Length < 1)
                throw new ArgumentException("The given basefolder path is null or too short.");

            if(assemblyBaseFolder.EndsWith(Path.DirectorySeparatorChar.ToString() )
                || assemblyBaseFolder.EndsWith(Path.AltDirectorySeparatorChar.ToString() ))
            {
                assemblyBaseFolder = assemblyBaseFolder.Remove(assemblyBaseFolder.Length - 1);
            }

            // 
            return Directory.GetParent(assemblyBaseFolder).Parent.FullName;
        }


    }
}
