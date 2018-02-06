using CommonAppData;
using MVVMApplication.View;
using System;
using System.Reflection;
using System.Windows;

namespace MVVMApplication
{
    public partial class FrontWindow : Window
    {
        public FrontWindow()
        {
            InitializeComponent();
            SetDataDirLocation();
        }

        private void Plain_Click(object sender, RoutedEventArgs e)
        {
            //MainWindowPlain mwp = new MainWindowPlain();
            //mwp.ShowDialog();
        }

        private void PropBagProxyEmit_Click(object sender, RoutedEventArgs e)
        {
            ShowMain("Emit_Proxy");
        }

        private void PropBagExtraMembers_Click(object sender, RoutedEventArgs e)
        {
            ShowMain("Extra_Members");
        }

        private void ShowMain(string configPackageNameSuffix)
        {
            MainWindow mw = new MainWindow(configPackageNameSuffix);
            mw.ShowDialog();
        }

        private void SetDataDirLocation()
        {
            Assembly thisAssembly = Assembly.GetEntryAssembly();
            CommonApplicationData cad = new CommonApplicationData(thisAssembly, allUsers: false);
            AppDomain.CurrentDomain.SetData("DataDirectory", cad.ApplicationFolderPath);
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
