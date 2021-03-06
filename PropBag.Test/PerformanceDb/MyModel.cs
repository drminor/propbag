﻿using PropBagLib.Tests.BusinessModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PropBagLib.Tests.PerformanceDb
{
    public class MyModel
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

        public double Size { get; set; }
    }

    public class MyModel2
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

        public double Size { get; set; }
    }

    public class MyModel3 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Guid _productId;
        public Guid ProductId
        {
            get { return _productId; }
            set
            {
                if (value != _productId)
                {
                    _productId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }
        }

        int _amount;
        public int Amount
        {
            get { return _amount; }
            set
            {
                if (value != _amount)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        double _size;
        public double Size
        {
            get { return _size; }
            set
            {
                if (value != _size)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        MyModel4 _deep;
        public MyModel4 Deep
        {
            get
            {
                return _deep;
            }
            set
            {
                if (_deep != value)
                {
                    _deep = value;
                    OnPropertyChanged(nameof(Deep));
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MyModel4 : INotifyPropertyChanged
    {
        public MyModel4()
        {
            MyString = "Brand New.";
        }

        string _myString;
        public string MyString
        {
            get
            {
                return _myString;
            }
            set
            {
                if(_myString != value)
                {
                    _myString = value;
                    OnPropertyChanged(nameof(MyString));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MyModel5 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Guid _productId;
        public Guid ProductId
        {
            get { return _productId; }
            set
            {
                if (value != _productId)
                {
                    _productId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }
        }

        Business _business;
        public Business Business
        {
            get { return _business; }
            set
            {
                if (value != _business)
                {
                    _business = value;
                    OnPropertyChanged(nameof(Business));
                }
            }
        }

        public ObservableCollection<Person> PersonCollection { get; set; }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MyModel6 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Business _business;
        public Business Business
        {
            get { return _business; }
            set
            {
                if (value != _business)
                {
                    _business = value;
                    OnPropertyChanged(nameof(Business));
                }
            }
        }

        MyModel5 _childVM;
        public MyModel5 ChildVM
        {
            get { return _childVM; }
            set
            {
                _childVM = value;
                OnPropertyChanged(nameof(ChildVM));
            }
        }

        Person _selectedPerson;
        public Person SelectedPerson
        {
            get { return _selectedPerson; }
            set
            {
                if(value != _selectedPerson)
                {
                    _selectedPerson = value;
                    OnPropertyChanged(nameof(SelectedPerson));
                }
            }
        }

        string _wMessage;
        public string WMessage
        {
            get { return _wMessage; }
            set
            {
                if(value != _wMessage)
                {
                    _wMessage = value;
                    OnPropertyChanged(nameof(WMessage));
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
