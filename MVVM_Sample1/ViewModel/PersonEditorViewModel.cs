using DRM.PropBag;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace MVVM_Sample1.ViewModel
{
    public partial class PersonEditorViewModel : PropBag
    {
        //int ITEMS_PER_PAGE = 10;
        string PROP_NAME = "PersonListView";
        List<RelayCommand> _commands;

        #region Command Handlers

        private void AddPerson(object o)
        {
            try
            {
                ListCollectionView lcv = GetIt<ListCollectionView>(PROP_NAME);

                if (TryGetViewManager(PROP_NAME, out IManageCViews cViewManager))
                {
                    PersonVM newPerson = (PersonVM)cViewManager.GetNewItem();
                    lcv.AddNewItem(newPerson);
                    lcv.MoveCurrentTo(newPerson);
                }

                //if (TryGetViewManager("Business", typeof(PersonDAL), out IManageCViews cViewManager))
                //{
                //    PersonVM newPerson = (PersonVM) cViewManager.GetNewItem();
                //    lcv.AddNewItem(newPerson);
                //    lcv.MoveCurrentTo(newPerson);
                //}
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine($"Could not get the view manager -- Fix this message.");
                //}
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        private void SavePerson(object o)
        {
            try
            {
                ListCollectionView lcv = GetIt<ListCollectionView>(PROP_NAME);
                if (lcv.IsEditingItem)
                {
                    lcv.CommitEdit();
                    ShowMessage("Changes are saved !");
                }
                else
                {
                    ShowMessage("No Pending Changes.");
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        private void DeletePerson(object o)
        {
            ListCollectionView lcv = GetIt<ListCollectionView>(PROP_NAME);

            PersonVM selectedPerson = (PersonVM)lcv.CurrentItem;
            if (selectedPerson == null) return;

            lcv.MoveCurrentToNext();

            lcv.Remove(selectedPerson);
            ShowMessage("Selected Person has been removed!");

            //ShowMessage("This is a test.");
        }

        //private void PageUpCom(object o)
        //{
        //    //ShowMessage("We Got a PageUp.");
        //    //if (--_page < 0) _page = 0;
        //    //FetchData(_bw, _page * ITEMS_PER_PAGE);
        //    ListCollectionView lcv = GetIt<ListCollectionView>(PROP_NAME);
        //    int pos = lcv.CurrentPosition;
        //    pos -= ITEMS_PER_PAGE;
        //    if (pos < 0) pos = 0;
        //    lcv.MoveCurrentToPosition(pos);
        //}

        //private void PageDownCom(object o)
        //{
        //    ////ShowMessage("We Got a PageDown.");
        //    ////if (++_page > 10) _page = 10;
        //    ////FetchData(_bw, _page * ITEMS_PER_PAGE);

        //    ListCollectionView lcv = GetIt<ListCollectionView>(PROP_NAME);
        //    int pos = lcv.CurrentPosition;
        //    pos += ITEMS_PER_PAGE;
        //    if (pos > lcv.Count - ITEMS_PER_PAGE) pos = lcv.Count - ITEMS_PER_PAGE;
        //    lcv.MoveCurrentToPosition(pos);
        //}

        private void ShowMessage(string msg)
        {
            string x = null;
            SetIt<string>(msg, ref x, "WMessage");
        }

        public void RefreshIt(object o)
        {
            if (TryGetViewManager(PROP_NAME, out IManageCViews cViewManager))
            {
                cViewManager?.GetDefaultCollectionView()?.Refresh();
            }
        }

        private bool TryGetViewManager(string propertyName, out IManageCViews cViewManager)
        {
            if (TryGetCViewManagerProvider(propertyName, out IProvideACViewManager cViewManagerProvider))
            {
                cViewManager = cViewManagerProvider.CViewManager;
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Could not get the view manager provider for {PROP_NAME}.");
                cViewManager = null;
                return false;
            }
        }

        #endregion

        #region Command Relayers

        public RelayCommand Add
        {
            get
            {
                RelayCommand addPersonRelayCmd = new RelayCommand(AddPerson, CanAddNewItem);
                addPersonRelayCmd.CanExecuteChanged += AddPersonRelayCmd_CanExecuteChanged;

                _commands.Add(addPersonRelayCmd);
                return addPersonRelayCmd;
            }
        }

        private bool CanAddNewItem()
        {
            //if (_wereDisposed) return false;

            try
            {
                if (TryGetViewManager(PROP_NAME, out IManageCViews cViewManager))
                {
                    if (cViewManager == null || cViewManager.IsDataSourceReadOnly())
                    {
                        return false;
                    }

                    if (cViewManager.GetDefaultCollectionView() is IEditableCollectionView iecv)
                    {
                        bool result = !(iecv.IsAddingNew || iecv.IsEditingItem);
                        return result;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ee)
            {
                // Ignore all exceptions.
                string a = ee.Message;
                return false;
            }
        }

        private void AddPersonRelayCmd_CanExecuteChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("The state of 'CanExecute' has changed for the Add command.");
        }

        public RelayCommand Save
        {
            get
            {
                RelayCommand savePersonRelayCommand = new RelayCommand(SavePerson);

                _commands.Add(savePersonRelayCommand);
                return savePersonRelayCommand;
            }
        }

        public RelayCommand Delete // => new RelayCommand(DeletePerson);
        {
            get
            {
                RelayCommand deletePersonRelayCommand = new RelayCommand(DeletePerson);

                _commands.Add(deletePersonRelayCommand);
                return deletePersonRelayCommand;
            }
        }


        //public RelayCommand PageUp => new RelayCommand(PageUpCom);

        //public RelayCommand PageDown => new RelayCommand(PageDownCom);

        public RelayCommand Refresh// => new RelayCommand(RefreshIt);
        {
            get
            {
                RelayCommand refreshRelayCommand = new RelayCommand(RefreshIt);

                _commands.Add(refreshRelayCommand);
                return refreshRelayCommand;
            }
        }

        #endregion
    }
}
