using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

namespace SetupCustomAction
{
    [RunInstaller(true)]
    public class ExceptionFilterInstaller : Installer
    {
        private const string AddinName = @"ExceptionFilter.Addin";

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            Assembly asm = Assembly.GetExecutingAssembly();
            string addInFileName = Path.Combine(Path.GetDirectoryName(asm.Location), AddinName);
            FileInfo addInFileInfo = new FileInfo(addInFileName);
            FileSecurity security = addInFileInfo.GetAccessControl();
            FileSystemAccessRule rule = new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Allow);
            security.SetAccessRule(rule);
            File.SetAccessControl(addInFileInfo.FullName, security);

            if (!addInFileInfo.Exists)
            {
                throw new FileNotFoundException(String.Format("File '{0}' is not found.", addInFileName));
            }

            try
            {
                XmlDocument doc = LoadAddInDocument(addInFileInfo);
                string folderPath = AddinFolderPath();
                CreateAddInDirectory(folderPath);
                DirectoryInfo addInDirectoryInfo = new DirectoryInfo(folderPath);
                bool modifiedSecurity = AddDirectorySecurity(addInDirectoryInfo.FullName, Environment.UserName, FileSystemRights.Write, AccessControlType.Allow);
                string destFile = Path.Combine(addInDirectoryInfo.FullName, AddinName);
                doc.Save(destFile);
                if (modifiedSecurity)
                {
                    RemoveDirectorySecurity(addInDirectoryInfo.FullName, Environment.UserName, FileSystemRights.Write, AccessControlType.Allow);
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to process file '{0}'.", addInFileInfo.FullName), e);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            try
            {
                string folderPath = AddinFolderPath();
                if (Directory.Exists(folderPath))
                {
                    string destFile = Path.Combine(folderPath, AddinName);
                    File.Delete(destFile);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        private static XmlDocument LoadAddInDocument(FileInfo fInfo)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fInfo.FullName);
            XPathNavigator navigator = doc.CreateNavigator();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(navigator.NameTable);
            nsmgr.AddNamespace("ae", "http://schemas.microsoft.com/AutomationExtensibility");

            XmlElement root = doc.DocumentElement;
            if (root == null)
            {
                throw new XmlSyntaxException("Addin XML format is invalid.");
            }

            XmlNode assemblyNode = root.SelectSingleNode("/ae:Extensibility/ae:Addin/ae:Assembly", nsmgr);
            if (assemblyNode == null)
            {
                throw new XmlSyntaxException("Addin XML format is invalid.");
            }

            assemblyNode.InnerText = Path.ChangeExtension(fInfo.FullName, "dll");
            return doc;
        }

        private static void CreateAddInDirectory(string folderPath)
        {
            DirectoryInfo parent = Directory.GetParent(folderPath);
            if (!parent.Exists)
            {
                CreateAddInDirectory(parent.FullName);
            }

            bool modifiedParentSecurity = AddDirectorySecurity(parent.FullName, Environment.UserName, FileSystemRights.Write, AccessControlType.Allow);
            Directory.CreateDirectory(folderPath);
            if (modifiedParentSecurity)
            {
                RemoveDirectorySecurity(parent.FullName, Environment.UserName, FileSystemRights.Write, AccessControlType.Allow);
            }
        }

        /// <summary>
        /// ["ALLUSERS"] is passed via the customactiondata in the setup project
        /// if ["ALLUSERS"] is not set or ="2", do a per user install
        /// see http://msdn.microsoft.com/en-us/library/aa367559(VS.85).aspx
        /// </summary>
        private string AddinFolderPath()
        {
            string alluser = this.Context.Parameters["ALLUSERS"];
            if (string.IsNullOrEmpty(alluser) || (string.Compare(alluser, "2") == 0))
            {
                string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return String.Concat(baseFolder, @"\Visual Studio 2010\AddIns");
            }
            else
            {
                string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                return String.Concat(baseFolder, @"\Microsoft\VisualStudio\10.0\Addins");
            }
        }

        /// Adds an ACL entry on the specified file for the specified account
        private static bool AddDirectorySecurity(string dirName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            DirectorySecurity dSecurity = Directory.GetAccessControl(dirName);
            FileSystemAccessRule rule = new FileSystemAccessRule(account, rights, controlType);
            bool modified = false;
            dSecurity.ModifyAccessRule(AccessControlModification.Add, rule, out modified);
            return modified;
        }

        /// Removes an ACL entry on the specified file for the specified account
        private static void RemoveDirectorySecurity(string dirName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            DirectorySecurity dSecurity = Directory.GetAccessControl(dirName);
            dSecurity.RemoveAccessRule(new FileSystemAccessRule(account, rights, controlType));
            Directory.SetAccessControl(dirName, dSecurity);
        }
    }
}
