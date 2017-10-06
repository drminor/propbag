using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DRM.TypeSafePropertyBag;

using System.Threading;

namespace DRM.PropBag 
{
    public class PropBagDyn : DynamicObject, IPubPropBag
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedWithValsHandler PropertyChangedWithVals;

        public bool AllPropsMustBeRegistered => propBag.AllPropsMustBeRegistered;

        public bool OnlyTypedAccess => propBag.OnlyTypedAccess;

        public ReadMissingPropPolicyEnum ReadMissingPropPolicy => propBag.ReadMissingPropPolicy;

        public bool ReturnDefaultForUndefined => propBag.ReturnDefaultForUndefined;

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
                propBag.SetValWithNoType(pName, value);
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
                // TOOD: Figure out way to retrieve type from configuration data.
                result = propBag.GetValWithType(pName, null);
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

        public IProp<T> AddProp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null, object extraInfo = null, T initialValue = default(T))
        {
            IProp<T> prop = propBag.AddProp(propertyName, doIfChanged, doAfterNotify, comparer, extraInfo, initialValue);
            RegisterDynProp(propertyName, initialValue);
            return prop;
        }

        public IProp<T> AddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            IProp<T> prop = propBag.AddPropNoStore<T>(propertyName, doIfChanged, doAfterNotify, comparer, extraInfo);
            // TODO: This is not quite right.
            RegisterDynProp(propertyName, default(T));
            return prop;
        }

        public IProp<T> AddPropNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            IProp<T> prop = propBag.AddPropNoValue<T>(propertyName, doIfChanged, doAfterNotify, comparer, extraInfo);
            // TODO: This is not quite right.
            RegisterDynProp(propertyName, default(T));
            return prop;
        }

        public IProp<T> AddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, object extraInfo = null, T initialValue = default(T))
        {
            IProp<T> prop = propBag.AddPropObjComp<T>(propertyName, doIfChanged, doAfterNotify, extraInfo, initialValue);
            RegisterDynProp(propertyName, default(T));
            return prop;
        }

        public IProp<T> AddPropObjCompNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false, object extraInfo = null)
        {
            IProp<T> prop = propBag.AddPropObjCompNoStore<T>(propertyName, doIfChanged, doAfterNotify, extraInfo);
            // TODO: This is not quite right.
            RegisterDynProp(propertyName, default(T));
            return prop;
        }

        public IProp<T> AddPropObjCompNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false, object extraInfo = null)
        {
            IProp<T> prop = propBag.AddPropObjCompNoValue<T>(propertyName, doIfChanged, doAfterNotify, extraInfo);
            // TODO: This is not quite right.
            RegisterDynProp(propertyName, default(T));
            return prop;
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

        public object this[string propertyName]
        {
            get
            {
                return GetValWithType(propertyName);
            }
            set
            {
                SetValWithType(propertyName, null, value);
            }
        }

        public object this[string typeName, string propertyName]
        {
            get
            {
                return GetValWithType(propertyName, Type.GetType(propertyName));
            }
            set
            {
                Type type = Type.GetType(typeName);
                SetValWithType(propertyName, type, value);
            }
        }

        public object GetValWithNoType(string propertyName)
        {
            return propBag.GetValWithType(propertyName, null);
        }

        public object GetValWithType(string propertyName, Type propertyType = null)
        {
            return propBag.GetValWithType(propertyName, propertyType);
        }

        public IPropGen GetPropGen(string propertyName, Type propertyType)
        {
            return propBag.GetPropGen(propertyName, propertyType);
        }

        public ValPlusType GetValPlusType(string propertyName, Type propertyType)
        {
            return propBag.GetValPlusType(propertyName, propertyType);
        }

        public T GetIt<T>(string propertyName)
        {
            return propBag.GetIt<T>(propertyName);
        }

        public IProp<T> GetTypedProp<T>(string propertyName)
        {
            return propBag.GetTypedProp<T>(propertyName);
        }

        public Type GetTypeOfProperty(string propertyName)
        {
            return propBag.GetTypeOfProperty(propertyName);
        }

        public bool PropertyExists(string propertyName)
        {
            return propBag.PropertyExists(propertyName);
        }

        public bool RegisterDoWhenChanged<T>(string propertyName, Action<T, T> doWhenChanged, bool doAfterNotify = false)
        {
            return propBag.RegisterDoWhenChanged<T>(propertyName, doWhenChanged, doAfterNotify);
        }

        public void RemoveProp(string propertyName)
        {
            propBag.RemoveProp(propertyName);
        }

        public bool SetIt<T>(T value, string propertyName)
        {
            return propBag.SetIt<T>(value, propertyName);
        }

        public bool SetIt<T>(T newValue, ref T curValue, string propertyName)
        {
            return propBag.SetIt<T>(newValue, ref curValue, propertyName);
        }

        public bool SetValWithNoType(string propertyName, object value)
        {
            return propBag.SetValWithNoType(propertyName, value);
        }


        public bool SetValWithType(string propertyName, Type propertyType, object value)
        {
            return propBag.SetValWithType(propertyName, propertyType, value);
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


        public new void AddToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, [CallerMemberName] string eventPropertyName = null)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            SubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        public new void RemoveFromPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, [CallerMemberName] string eventPropertyName = null)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            UnSubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        /// <summary>
        /// Given a string in the form "{0}Changed", where {0} is the underlying property name, parse out and return the value of {0}.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected string GetPropNameFromEventProp(string x)
        {
            //PropStringChanged
            return x.Substring(0, x.Length - 7);
        }

        public object GetValueGen(object host, string propertyName, Type propertyType)
        {
            return ((IPropBag)host).GetValWithType(propertyName, propertyType);
        }

        public void SetValueGen(object host, string propertyName, Type propertyType, object value)
        {
            ((IPropBag)host).SetValWithType(propertyName, propertyType, value);
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
