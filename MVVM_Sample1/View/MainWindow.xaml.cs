using DRM.TypeSafePropertyBag;
using MVVMApplication.Infra;
using MVVMApplication.ViewModel;
using System;
using System.Windows;

namespace MVVMApplication.View
{
    public partial class MainWindow : Window
    {
        MainWindowViewModel OurData => (MainWindowViewModel)this.DataContext;

        public MainWindow(string packageConfigName)
        {
            System.Diagnostics.Debug.WriteLine("Just before MainWindow InitComp.");

            // TODO: Is there a more formal way of handling this global setting?
            PropStoreServicesForThisApp.PackageConfigName = packageConfigName;

            InitializeComponent();

            Closed += Window_Closed;
            OurData.SubscribeToPropChanged<string>(WMessageHasArrived, "WMessage");

            OurData.RequestClose += OurData_RequestClose;

            System.Diagnostics.Debug.WriteLine("Just afer MainWindow InitComp.");
        }

        // A message has been sent to us, most likey from our ViewModel, but perhaps from an ancestor or descendant ViewModel.
        private void WMessageHasArrived(object sender, PcTypedEventArgs<string> e)
        {
            if (e.NewValue != null)
            {
                MessageBox.Show(e.NewValue, "MVVM Application", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            //OurData.SetIt<string>(null, "WMessage");
        }

        // Our ViewModel is requesting us to close.
        private void OurData_RequestClose(object sender, EventArgs e)
        {
            Close();
        }

        // We have been closed.
        private void Window_Closed(object sender, EventArgs e)
        {
            OurData?.Dispose();
        }
    }
}
