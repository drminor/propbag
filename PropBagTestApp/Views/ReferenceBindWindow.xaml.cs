using PropBagTestApp.Models;
using System.Windows;
using System;
using PropBagTestApp.ViewModels;

namespace PropBagTestApp.View
{
    /// <summary>
    /// Interaction logic for ReferenceBindWindow.xaml
    /// </summary>
    public partial class ReferenceBindWindow : Window
    {
        public ReferenceBindWindow()
        {
            InitializeComponent();

            ReferenceBindViewModel rbvm = new ReferenceBindViewModel()
            {
                Size = 90.11,
                Amount = 51,
                ProductId = Guid.NewGuid(),
                TestDouble = 90.33,
                Deep = new MyModel4() { MyString = "New Values." }
            };

            this.DataContext = rbvm;
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModel m = (ReferenceBindViewModel)this.DataContext;
            m.Amount = 21;
            m.Size = 30.3;
            m.ProductId = Guid.NewGuid();
            m.TestDouble = 0.01;
            m.Deep = new MyModel4() { MyString = "hello" };
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModel m = (ReferenceBindViewModel)this.DataContext;

            System.Diagnostics.Debug.WriteLine($"Size = {m.Size}.");
            System.Diagnostics.Debug.WriteLine($"Deep.MyString = {m.Deep.MyString}.");
        }
    }
}
