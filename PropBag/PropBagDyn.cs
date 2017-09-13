using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DRM.Inpcwv;
using System.Threading;

namespace DRM.PropBag 
{
    public class PropBagDyn : DynamicObject, IPubPropBag
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedWithValsHandler PropertyChangedWithVals;

        private PubPropBag propBag;

        #region Constructors

        public PropBagDyn()
        {
            propBag = new PubPropBag(); // PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            System.Diagnostics.Debug.Assert(propBag.TypeSafetyMode != PropBagTypeSafetyMode.OnlyTypedAccess, "OnlyTypeAccess is not supported for PropBagDyn.");
            InterceptEvents(propBag);
        }

        public PropBagDyn(byte dummy)
        {
            propBag = new PubPropBag(dummy);
        }

        public PropBagDyn(PropBagTypeSafetyMode typeSafetyMode)
        {
            if (typeSafetyMode == PropBagTypeSafetyMode.OnlyTypedAccess)
                throw new ApplicationException("OnlyTypedAccess is not supported.");

            propBag = new PubPropBag(typeSafetyMode);
            InterceptEvents(propBag);
        }

        public PropBagDyn(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory thePropFactory)
        {
            if (typeSafetyMode == PropBagTypeSafetyMode.OnlyTypedAccess)
                throw new ApplicationException("OnlyTypedAccess is not supported.");

            propBag = new PubPropBag(typeSafetyMode, thePropFactory);
            InterceptEvents(propBag);
        }

        public PropBagDyn(ControlModel.PropModel pm)
        {
            propBag = new PubPropBag(pm);
            System.Diagnostics.Debug.Assert(propBag.TypeSafetyMode != PropBagTypeSafetyMode.OnlyTypedAccess, "OnlyTypeAccess is not supported for PropBagDyn.");
            InterceptEvents(propBag);
        }

        private void InterceptEvents(IPropBag pb)
        {
            pb.PropertyChanged += Pb_PropertyChanged;
            pb.PropertyChanging += Pb_PropertyChanging;
            pb.PropertyChangedWithVals += Pb_PropertyChangedWithVals;
        }

        private void Pb_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            OnPropertyChanging(e);
        }

        private void Pb_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        private void Pb_PropertyChangedWithVals(object sender, PropertyChangedWithValsEventArgs e)
        {
            OnPropertyChangedWithVals(e);
        }

        // Raise Standard Events
        private void OnPropertyChanged(PropertyChangedEventArgs eArgs)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

            if (handler != null)
            {
                handler(this, eArgs);
            }
        }

        private void OnPropertyChanging(PropertyChangingEventArgs eArgs)
        {
            PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);

            if (handler != null)
            {
                handler(this, eArgs);
            }
        }

        private void OnPropertyChangedWithVals(PropertyChangedWithValsEventArgs eArgs)
        {
            PropertyChangedWithValsHandler handler = Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null);

            if (handler != null)
                handler(this, eArgs);
        }

        #endregion

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string pName = binder.Name;

            try
            {
                propBag.SetItWithNoType(value, pName);
                return true;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            string pName = binder.Name;

            try
            {
                result = propBag.GetIt(pName);
                return true;
            } 
            catch (System.Exception e)
            {
                return false;
            }
        }

        public PropBagTypeSafetyMode TypeSafetyMode
        {
            get
            {
                return propBag.TypeSafetyMode;
            }
        }

        #region Add Property Methods

        public IProp<T> AddProp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null, object extraInfo = null, T initalValue = default(T))
        {
            RegisterDynProp(propertyName, initalValue);
            return propBag.AddProp(propertyName, doIfChanged, doAfterNotify, comparer, extraInfo, initalValue);
        }

        public IProp<T> AddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            throw new NotImplementedException();
        }

        public IProp<T> AddPropNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            throw new NotImplementedException();
        }

        public IProp<T> AddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, object extraInfo = null, T initalValue = default(T))
        {
            throw new NotImplementedException();
        }

        public IProp<T> AddPropObjCompNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false, object extraInfo = null)
        {
            throw new NotImplementedException();
        }

        public IProp<T> AddPropObjCompNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, object extraInfo = null)
        {
            throw new NotImplementedException();
        }

        private bool RegisterDynProp(string propertyName, object value)
        {
            MySetMemberBinder binder = new MySetMemberBinder(propertyName, false);
            return this.TrySetMember(binder, value);
        }

        #endregion

        public void ClearAll()
        {
            propBag.ClearAll();
        }

        public void ClearEventSubscribers()
        {
            propBag.ClearEventSubscribers();
        }

        public IList<string> GetAllPropertyNames()
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetAllPropertyValues()
        {
            return propBag.GetAllPropertyValues();
        }

        public IDictionary<string, ValPlusType> GetAllPropNamesAndTypes()
        {
            return propBag.GetAllPropNamesAndTypes();
        }

        public object this[string typeName, string propertyName]
        {
            get
            {
                return GetIt(propertyName, Type.GetType(propertyName));
            }
            set
            {
                Type type = Type.GetType(typeName);
                SetItWithType(value, type, propertyName);
            }
        }

        public object GetIt([CallerMemberName] string propertyName = null, Type propertyType = null)
        {
            return propBag.GetIt(propertyName, propertyType);
        }

        public T GetIt<T>([CallerMemberName] string propertyName = null)
        {
            return propBag.GetIt<T>(propertyName);
        }

        public Type GetTypeOfProperty(string propertyName)
        {
            return propBag.GetTypeOfProperty(propertyName);
        }

        public bool PropertyExists(string propertyName)
        {
            return propBag.PropertyExists(propertyName);
        }

        public bool RegisterDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify = false, [CallerMemberName] string propertyName = null)
        {
            return propBag.RegisterDoWhenChanged<T>(doWhenChanged, doAfterNotify, propertyName);
        }

        public void RemoveProp(string propertyName)
        {
            propBag.RemoveProp(propertyName);
        }

        public bool SetIt<T>(T value, [CallerMemberName] string propertyName = null)
        {
            return propBag.SetIt<T>(value, propertyName);
        }

        public bool SetIt<T>(T newValue, ref T curValue, [CallerMemberName] string propertyName = null)
        {
            return propBag.SetIt<T>(newValue, ref curValue, propertyName);
        }

        public bool SetItWithNoType(object value, [CallerMemberName] string propertyName = null)
        {
            return propBag.SetItWithNoType(value, propertyName);
        }

        public bool SetItWithType(object value, Type propertyType = null, [CallerMemberName] string propertyName = null)
        {
            return propBag.SetItWithType(value, propertyType, propertyName);
        }

        public void SubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName)
        {
            propBag.SubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        public void SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName)
        {
            propBag.SubscribeToPropChanged<T>(doOnChange, propertyName);
        }

        public void SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        {
            propBag.SubscribeToPropChanged(doOnChange, propertyName);
        }

        public void UnSubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName)
        {
            propBag.UnSubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        public bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName)
        {
            return propBag.UnSubscribeToPropChanged<T>(doOnChange, propertyName);
        }

        public bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        {
            return propBag.UnSubscribeToPropChanged(doOnChange, propertyName);
        }
    }

    public class MySetMemberBinder : SetMemberBinder
    {
        public MySetMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase) { }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}
