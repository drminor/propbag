using System;
using System.Reflection;

/// <remarks>
/// 
/// A local one way binding from source to target is a short hand way of
///     1. Wiring an eventhandler to the event fired by the source when the specified property changes.
///     2. Having the body of that event handler use the target property's set accessor to update
///         the "bound" value.
///         
/// We then say that the target's property is bound to the source's property.
/// 
/// 
/// The source is some object that supports INotifyPropertyChanged, or INotifyPCTyped<T> 
/// The target is an IProp or IProp[T] on an IPropBag.
///
/// </remarks>

namespace DRM.TypeSafePropertyBag
{
    public class SimpleLocalBindingOps<PropDataT> : ICreateLocalBindings<PropDataT> where PropDataT : IPropGen
    {
        #region Private Mmbers

        SimpleSubscriptionManager<PropDataT> _subScriptionManager;

        private SimpleLevel2KeyMan _level2KeyManager;
        private SimpleCompKeyMan _compKeyManager;
        private readonly SimpleObjectIdDictionary<PropDataT> _propertyStore;

        #endregion

        #region Constructors

        public SimpleLocalBindingOps()
        {

        }

        public SimpleLocalBindingOps
            (
            SimpleSubscriptionManager<PropDataT> subScriptionManager,
            SimpleLevel2KeyMan level2KeyManager,
            SimpleCompKeyMan compKeyManager,
            SimpleObjectIdDictionary<PropDataT> propertyStore
            )
        {
            _subScriptionManager = subScriptionManager ?? throw new ArgumentNullException(nameof(subScriptionManager));
            _level2KeyManager = level2KeyManager;
            _compKeyManager = compKeyManager;
            _propertyStore = propertyStore;
        }

        #endregion

        #region Public Methods

        public Action<SourceT, SourceT> CreateLBinding<SourceT>(SimpleExKey source, SimpleExKey target, LocalBindingInfo lbInfo)
        {
            throw new NotImplementedException();
        }

        public Action<SourceT, SourceT> CreateLBinding<SourceT, TargetT>
            (
            SimpleExKey source,
            SimpleExKey target,
            LocalBindingInfo lbInfo
            )
        {
            Action<SourceT, SourceT> action = UpdateTarget;
            return action;
        }

        #endregion

        #region Private Methods

        private void UpdateTarget<SourceT>(SourceT x, SourceT y)
        {

        }

        #endregion

        #region Delegate Creation Logic

        static private Type gmtType = typeof(LBOGenericMethodTemplates);

        public ICacheSubscriptions<SimpleExKey, ulong, uint, uint, IPropGen> SubscriptionManager => throw new NotImplementedException();


        // From Object
        //protected virtual CreatePropFromObjectDelegate GetPropCreator(Type typeOfThisValue)
        //{
        //    MethodInfo mi = gmtType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
        //    CreatePropFromObjectDelegate result = (CreatePropFromObjectDelegate)Delegate.CreateDelegate(typeof(CreatePropFromObjectDelegate), mi);

        //    return result;
        //}

        #endregion
    }

    static class LBOGenericMethodTemplates
    {
        #region Property-Type Methods

        // From Object
        //private static IProp<T> CreatePropFromObject<T>(IPropFactory propFactory,
        //    object value,
        //    string propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid,
        //    EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        //{
        //    IProp<T> result = null;

        //    return result;
        //}

        #endregion
    }
}
