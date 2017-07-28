using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropBagLib.Tests
{
    public partial class SetAndGetModel
    {
        public object PropObject
        {
            get { return GetIt<object>(); }
            set { SetIt<object>(value); }
        }

        public string PropString
        {
            get { return GetIt<string>(); }
            set { SetIt<string>(value); }
        }

        // Simple Type (also a struct)
        public bool PropBool
        {
            get { return GetIt<bool>(); }
            set { SetIt<bool>(value); }
        }

        // Simple Type (also a struct)
        public int PropInt
        {
            get { return GetIt<int>(); }
            set { SetIt<int>(value); }
        }

        // Struct
        public TimeSpan PropTimeSpan
        {
            get { return GetIt<TimeSpan>(); }
            set { SetIt<TimeSpan>(value); }
        }

        // Class instance
        public Uri PropUri
        {
            get { return GetIt<Uri>(); }
            set { SetIt<Uri>(value); }
        }

        // Class instance from generic
        public Lazy<int> PropLazyInt
        {
            get { return GetIt<Lazy<int>>(); }
            set { SetIt<Lazy<int>>(value); }
        }
    }

    public partial class RefEqualityModel
    {
        public string PropString
        {
            get { return GetIt<string>(); }
            set { SetIt<string>(value); }
        }

        public void SubscribeToPropStringChanged(Action<string, string> action)
        {
            SubscribeToPropChanged<string>(action, "PropString");
        }

    }

    public partial class NullableModel
    {
        public Nullable<int> PropNullableInt
        {
            get { return GetIt<Nullable<int>>(); }
            set { SetIt<Nullable<int>>(value);  }
        }

        public ICollection<int> PropICollectionInt
        {
            get { return GetIt<ICollection<int>>(); }
            set { SetIt<ICollection<int>>(value); }
        }
    }

    public partial class SandGLoosetModel
    {

        public void SubscribeToPropStringChanged(Action<string, string> action)
        {
            SubscribeToPropChanged<string>(action, "PropString");
        }

        public object PropObject
        {
            get { return GetIt<object>(); }
            set { SetIt<object>(value); }
        }

        public string PropString
        {
            get { return GetIt<string>(); }
            set { SetIt<string>(value); }
        }

        // Simple Type (also a struct)
        public bool PropBool
        {
            get { return GetIt<bool>(); }
            set { SetIt<bool>(value); }
        }

        // Simple Type (also a struct)
        public int PropInt
        {
            get { return GetIt<int>(); }
            set { SetIt<int>(value); }
        }

        // Struct
        public TimeSpan PropTimeSpan
        {
            get { return GetIt<TimeSpan>(); }
            set { SetIt<TimeSpan>(value); }
        }

        // Class instance
        public Uri PropUri
        {
            get { return GetIt<Uri>(); }
            set { SetIt<Uri>(value); }
        }

        // Class instance from generic
        public Lazy<int> PropLazyInt
        {
            get { return GetIt<Lazy<int>>(); }
            set { SetIt<Lazy<int>>(value); }
        }
    }

}
