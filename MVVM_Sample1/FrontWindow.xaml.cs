using CommonAppData;
using MVVM_Sample1.Infra;
using ObjectSizeDiagnostics;
using System;
using System.Reflection;
using System.Windows;

namespace MVVM_Sample1.View
{
    public partial class FrontWindow : Window
    {
        #region Private Members

        MemConsumptionTracker _mct = new MemConsumptionTracker(enabled: false);

        #endregion

        #region Constructor

        public FrontWindow()
        {
            InitializeComponent();
            SetDataDirLocation();
        }

        #endregion

        #region Event Handlers

        private void Plain_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "This is here to run the application using 'standard' MVVM WPF technology" +
                " as a base-line comparison, however currently the 'plain' MainWindow is not working.",
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);

            //MainWindowPlain mwp = new MainWindowPlain();
            //mwp.ShowDialog();
        }

        private void PropBagProxyEmit_Click(object sender, RoutedEventArgs e)
        {
            _mct.Enabled = true;
            _mct.CompactAndMeasure("Before Starting MainWindow (EmitProxy)");

            ShowMain("Emit");

            _mct.MeasureAndReport("The Main Window has been closed. Before Compaction.", "FrontWindow");
            _mct.CompactAndMeasure();
            _mct.MeasureAndReport("The Main Window has been closed. After Compaction.", "FrontWindow");
        }

        private void PropBagExtraMembers_Click(object sender, RoutedEventArgs e)
        {
            ShowMain("Extra");
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        #endregion

        #region Private Methods

        private void ShowMain(string configPackageNameSuffix)
        {
            //GetWindowList_Diag();
            PropStoreServicesForThisApp.ConfigPackageNameSuffix = configPackageNameSuffix;

            MainWindow mw = new MainWindow
            {
                Owner = GetWindow(this)
            };

            mw.ShowDialog();

            //WindowState ws = mw.WindowState;
            //GetWindowList_Diag();
        }

        private void SetDataDirLocation()
        {
            Assembly thisAssembly = Assembly.GetEntryAssembly();
            CommonApplicationData cad = new CommonApplicationData(thisAssembly, allUsers: false);
            AppDomain.CurrentDomain.SetData("DataDirectory", cad.ApplicationFolderPath);
        }

        private void GetWindowList_Diag()
        {
            System.Diagnostics.Debug.WriteLine("The list of open windows....");
            foreach (Window w in Application.Current.Windows)
            {
                System.Diagnostics.Debug.WriteLine(w.GetType().FullName);
            }
        }

        #endregion
    }
}
