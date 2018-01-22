using System;
using System.Data.Entity;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace CommonAppData
{
    public interface IHaveADbContext
    {
        DbContext DbContext { get; }
    }

    public class DBActivator<T> : IHaveADbContext where T: DbContext, new()
    {
        public T TypedDbContext { get; }

        public DbContext DbContext => TypedDbContext;

        public DBActivator()
        {
            TypedDbContext = GetNewDbContext();
        }

        public DBActivator(string dataDirPath)
        {
            SetDataDir(dataDirPath);
            TypedDbContext = GetNewDbContext();
        }

        public DBActivator(CommonApplicationData commonApplicationData)
        {
            SetDataDir(commonApplicationData.ApplicationFolderPath);
            TypedDbContext = GetNewDbContext();
        }

        public DBActivator(Environment.SpecialFolder specialFolder)
        {
            string dataDirPath = Environment.GetFolderPath(specialFolder);
            SetDataDir(dataDirPath);
            TypedDbContext = GetNewDbContext();
        }

        public DBActivator(Environment.SpecialFolder specialFolder, string subFolder)
        {
            string dataDirPath = Environment.GetFolderPath(specialFolder);
            dataDirPath = Path.Combine(dataDirPath, subFolder);

            SetDataDir(dataDirPath);
            TypedDbContext = GetNewDbContext();
        }

        public DBActivator(Environment.SpecialFolder specialFolder, string companyFolder, string appFolder)
        {
            string dataDirPath = Environment.GetFolderPath(specialFolder);
            dataDirPath = Path.Combine(dataDirPath, companyFolder, appFolder);
            SetDataDir(dataDirPath);
            TypedDbContext = GetNewDbContext();
        }

        public DBActivator(Assembly assembly)
        {
            string companyName = GetAssemblyCompany(assembly);
            string assemblyId = GetAssemblyId(assembly);

            CommonApplicationData cad = new CommonApplicationData(companyName, assemblyId, allUsers: false);
            SetDataDir(cad.ApplicationFolderPath);
            TypedDbContext = GetNewDbContext();
        }

        private string GetAssemblyId(Assembly assembly)
        {
            object[] attr = (assembly.GetCustomAttributes(typeof(GuidAttribute), true));
            Guid guid = new Guid((attr[0] as GuidAttribute).Value);
            string result = guid.ToString("B").ToUpper();
            return result;
        }

        private string GetAssemblyCompany(Assembly assembly)
        {
            object[] attr = (assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true));
            string result = ((AssemblyCompanyAttribute)attr[0]).Company;
            return result;
        }

        private void SetDataDir(string dataDirPath)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirPath);
        }

        private T GetNewDbContext()
        {
            T result = new T();
            return result;
        }
    }
}
