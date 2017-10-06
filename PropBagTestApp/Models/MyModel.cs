using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

        public double Size { get; set; }

        MyModel4 _deep;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
