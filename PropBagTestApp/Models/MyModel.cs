using System;
using System.ComponentModel;

namespace PropBagTestApp.Models
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
                if(value != _productId)
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
                if(_deep != value)
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
}
