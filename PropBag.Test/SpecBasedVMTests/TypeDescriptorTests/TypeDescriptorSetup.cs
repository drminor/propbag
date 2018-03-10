using CommonAppData;
using System;
using System.Reflection;

namespace PropBagLib.Tests.SpecBasedVMTests.TypeDescriptorTests
{
    public abstract class TypeDescriptorSetup : PropBagVMTestBase
    {
        protected override Action EstablishContext()
        {
            DataDirPath = GetDataDirLocation();
            UpdateDataDirSetting(DataDirPath);

            return base.EstablishContext();
        }

        private string GetDataDirLocation()
        {
            Assembly thisAssembly = Assembly.GetCallingAssembly();
            CommonApplicationData cad = new CommonApplicationData(thisAssembly, allUsers: false);

            return cad.ApplicationFolderPath;
        }

        private void UpdateDataDirSetting(string path)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
        }

    }
}
