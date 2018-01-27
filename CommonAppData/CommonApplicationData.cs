using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;


/// <remarks>
/// Copied from: https://www.codeproject.com/Tips/61987/Allow-write-modify-access-to-CommonApplicationData
/// Author: Dave Meadowcroft, https://www.codeproject.com/Members/DaveyM69
/// License: Code Project Open License (CPOL)
/// </remarks>>

namespace CommonAppData
{
    /// <summary>
    /// Simplifies the creation of folders in the CommonApplicationData folder
    /// and setting of permissions for all users.
    /// </summary>
    public class CommonApplicationData
    {
        private string applicationFolder;
        private string companyFolder;
        private static readonly string directory =
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        public CommonApplicationData(Assembly assembly)
            : this(GetAssemblyCompany(assembly), GetAssemblyId(assembly))
        {
        }

        public CommonApplicationData(Assembly assembly, bool allUsers)
            : this(GetAssemblyCompany(assembly), GetAssemblyId(assembly), allUsers)
        {
        }

        /// <summary>
        /// Creates a new instance of this class creating the specified company and application folders
        /// if they don't already exist and optionally allows write/modify to all users.
        /// </summary>
        /// <param name="companyFolder">The name of the company's folder (normally the company name).</param>
        /// <param name="applicationFolder">The name of the application's folder (normally the application name).</param>
        /// <remarks>If the application folder already exists then permissions if requested are NOT altered.</remarks>
        public CommonApplicationData(string companyFolder, string applicationFolder)
            : this(companyFolder, applicationFolder, false)
        { }

        /// <summary>
        /// Creates a new instance of this class creating the specified company and application folders
        /// if they don't already exist and optionally allows write/modify to all users.
        /// </summary>
        /// <param name="companyFolder">The name of the company's folder (normally the company name).</param>
        /// <param name="applicationFolder">The name of the application's folder (normally the application name).</param>
        /// <param name="allUsers">true to allow write/modify to all users; otherwise, false.</param>
        /// <remarks>If the application folder already exists then permissions if requested are NOT altered.</remarks>
        public CommonApplicationData(string companyFolder, string applicationFolder, bool allUsers)
        {
            this.companyFolder = companyFolder;
            this.applicationFolder = applicationFolder;
            CreateFolders(allUsers);
        }

        /// <summary>
        /// Gets the path of the application's data folder.
        /// </summary>
        public string ApplicationFolderPath
        {
            get { return Path.Combine(CompanyFolderPath, applicationFolder); }
        }
        /// <summary>
        /// Gets the path of the company's data folder.
        /// </summary>
        public string CompanyFolderPath
        {
            get { return Path.Combine(directory, companyFolder); }
        }

        private void CreateFolders(bool allUsers)
        {
            DirectoryInfo directoryInfo;
            DirectorySecurity directorySecurity;
            AccessRule rule;
            SecurityIdentifier securityIdentifier = new SecurityIdentifier
                (WellKnownSidType.BuiltinUsersSid, null);
            if (!Directory.Exists(CompanyFolderPath))
            {
                directoryInfo = Directory.CreateDirectory(CompanyFolderPath);
                directorySecurity = directoryInfo.GetAccessControl();
                rule = new FileSystemAccessRule(
                        securityIdentifier,
                        FileSystemRights.Write |
                        FileSystemRights.ReadAndExecute |
                        FileSystemRights.Modify,
                        AccessControlType.Allow);
                directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out bool modified);
                directoryInfo.SetAccessControl(directorySecurity);
            }
            if (!Directory.Exists(ApplicationFolderPath))
            {
                directoryInfo = Directory.CreateDirectory(ApplicationFolderPath);
                if (allUsers)
                {
                    directorySecurity = directoryInfo.GetAccessControl();
                    rule = new FileSystemAccessRule(
                        securityIdentifier,
                        FileSystemRights.Write |
                        FileSystemRights.ReadAndExecute |
                        FileSystemRights.Modify,
                        InheritanceFlags.ContainerInherit |
                        InheritanceFlags.ObjectInherit,
                        PropagationFlags.InheritOnly,
                        AccessControlType.Allow);
                    directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out bool modified);
                    directoryInfo.SetAccessControl(directorySecurity);
                }
            }
        }
        /// <summary>
        /// Returns the path of the application's data folder.
        /// </summary>
        /// <returns>The path of the application's data folder.</returns>
        public override string ToString()
        {
            return ApplicationFolderPath;
        }

        private static string GetAssemblyId(Assembly assembly)
        {
            object[] attr = (assembly.GetCustomAttributes(typeof(GuidAttribute), true));
            if(attr.Length == 0)
            {
                throw new InvalidOperationException("This assembly does not have a GUID.");
            }
            Guid guid = new Guid((attr[0] as GuidAttribute).Value);
            string result = guid.ToString("B").ToUpper();
            return result;
        }

        private static string GetAssemblyCompany(Assembly assembly)
        {
            object[] attr = (assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true));
            string result = ((AssemblyCompanyAttribute)attr[0]).Company;
            return result;
        }
    }
}