using System.Windows.Controls;

namespace MVVM_Sample1.View
{
    public partial class PersonCollection : UserControl
    {
        public PersonCollection()
        {
            InitializeComponent();
        }

        private void PersonListDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                // find row for the first selected item
                DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(e.AddedItems[0]);
                if (row != null && row.Item != null)
                {
                    dataGrid.ScrollIntoView(row.Item);
                }
            }
        }
    }
}
