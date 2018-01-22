using DRM.PropBag;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using MVVMApplication.Services;
using System;
using System.ComponentModel;
using System.Windows.Data;

namespace MVVMApplication.ViewModel
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class PersonCollectionViewModel : PropBag
    {
        //int ITEMS_PER_PAGE = 10;

        public PersonCollectionViewModel(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, propFactory, fullClassName)
        {
            System.Diagnostics.Debug.WriteLine("Constructing PersonCollectionViewModel -- with PropModel.");
        }

        //#region Command Handlers

        //private void AddPerson(object o)
        //{
        //    try
        //    {
        //        ListCollectionView lcv = GetIt<ListCollectionView>("PersonListView");

        //        if (TryGetViewManager("Business", typeof(PersonDAL), out IManageCViews cViewManager))
        //        {
        //            PersonVM newPerson = (PersonVM) cViewManager.GetNewItem();
        //            lcv.AddNewItem(newPerson);
        //            lcv.MoveCurrentTo(newPerson);
        //        }
        //        else
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Could not get the view manager -- Fix this message.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowMessage(ex.Message);
        //    }
        //}

        //private void SavePerson(object o)
        //{
        //    try
        //    {
        //        ListCollectionView lcv = GetIt<ListCollectionView>("PersonListView");
        //        if (lcv.IsEditingItem)
        //        {
        //            lcv.CommitEdit();
        //            ShowMessage("Changes are saved !");
        //        }
        //        else
        //        {
        //            ShowMessage("No Pending Changes.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowMessage(ex.Message);
        //    }
        //}

        //private void DeletePerson(object o)
        //{
        //    ListCollectionView lcv = GetIt<ListCollectionView>("PersonListView");

        //    PersonVM selectedPerson = (PersonVM)lcv.CurrentItem;
        //    if (selectedPerson == null) return;

        //    lcv.Remove(selectedPerson);
        //    ShowMessage("Selected Person has been removed!");
        //}

        ////private void PageUpCom(object o)
        ////{
        ////    //ShowMessage("We Got a PageUp.");
        ////    //if (--_page < 0) _page = 0;
        ////    //FetchData(_bw, _page * ITEMS_PER_PAGE);
        ////    ListCollectionView lcv = GetIt<ListCollectionView>("PersonListView");
        ////    int pos = lcv.CurrentPosition;
        ////    pos -= ITEMS_PER_PAGE;
        ////    if (pos < 0) pos = 0;
        ////    lcv.MoveCurrentToPosition(pos);
        ////}

        ////private void PageDownCom(object o)
        ////{
        ////    ////ShowMessage("We Got a PageDown.");
        ////    ////if (++_page > 10) _page = 10;
        ////    ////FetchData(_bw, _page * ITEMS_PER_PAGE);

        ////    ListCollectionView lcv = GetIt<ListCollectionView>("PersonListView");
        ////    int pos = lcv.CurrentPosition;
        ////    pos += ITEMS_PER_PAGE;
        ////    if (pos > lcv.Count - ITEMS_PER_PAGE) pos = lcv.Count - ITEMS_PER_PAGE;
        ////    lcv.MoveCurrentToPosition(pos);
        ////}

        //private void ShowMessage(string msg)
        //{
        //    string x = null;
        //    SetIt<string>(msg, ref x, "WMessage");
        //}

        //public void RefreshIt(object o)
        //{
        //    if (TryGetViewManager("Business", typeof(PersonDAL), out IManageCViews cViewManager))
        //    {
        //        cViewManager.GetDefaultCollectionView().Refresh();
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Could not get the view manager -- Fix this message.");
        //    }
        //}

        //#endregion

        //#region Command Relayers

        //public RelayCommand Add
        //{
        //    get
        //    {
        //        var x = new RelayCommand(AddPerson, CanAddNewItem);
        //        x.CanExecuteChanged += X_CanExecuteChanged;
        //        return x;
        //    }
        //}

        //private bool CanAddNewItem()
        //{
        //    if (TryGetViewManager("Business", typeof(PersonDAL), out IManageCViews cViewManager))
        //    {
        //        if(cViewManager.IsReadOnly())
        //        {
        //            return false;
        //        }

        //        if(cViewManager.GetDefaultCollectionView() is IEditableCollectionView iecv)
        //        {
        //            bool result = !(iecv.IsAddingNew || iecv.IsEditingItem);
        //            return result;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //private void X_CanExecuteChanged(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("The state of 'CanExecute' has changed for the Add command.");
        //}

        //public RelayCommand Save => new RelayCommand(SavePerson);

        //public RelayCommand Delete => new RelayCommand(DeletePerson);

        ////public RelayCommand PageUp => new RelayCommand(PageUpCom);

        ////public RelayCommand PageDown => new RelayCommand(PageDownCom);

        //public RelayCommand Refresh => new RelayCommand(RefreshIt);

        //#endregion
    }
}
