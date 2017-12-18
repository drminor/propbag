using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ControlModel 
{
    public class PropDoWhenChangedField : NotifyPropertyChangedBase, IEquatable<PropDoWhenChangedField>
    {
        //EventHandler<PcGenEventArgs> _dwc;
        //public EventHandler<PcGenEventArgs> DoWhenChangedAction { get { return _dwc; } set { SetIfDifferentDelegate<EventHandler<PcGenEventArgs>>(ref _dwc, value, nameof(DoWhenChangedAction)); } }

        //bool _doAfterNotify;
        //public bool DoAfterNotify { get { return _doAfterNotify; } set { SetIfDifferent<bool>(ref _doAfterNotify, value, nameof(DoAfterNotify)); } }

        bool _methodIsLocal;
        public bool MethodIsLocal { get { return _methodIsLocal; } set { SetIfDifferent<bool>(ref _methodIsLocal, value, nameof(MethodIsLocal)); } }

        Type _declaringType;
        public Type DeclaringType { get { return _declaringType; } set { _declaringType = value; } }

        string _fullClassName;
        public string FullClassName { get { return _fullClassName; } set { SetIfDifferent<string>(ref _fullClassName, value, nameof(FullClassName)); } }

        string _instanceKey;
        public string InstanceKey { get { return _instanceKey; } set { SetIfDifferent<string>(ref _instanceKey, value, nameof(InstanceKey)); } }

        string _methodName;
        public string MethodName { get { return _methodName; } set { SetIfDifferent<string>(ref _methodName, value, nameof(MethodName)); } }

        object _target;
        public object Target { get { return _target; } set { SetIfDifferentRefEqu<object>(ref _target, value, nameof(Target)); } }

        MethodInfo _method;
        public MethodInfo Method { get { return _method; } set { SetIfDifferentDelegate<MethodInfo>(ref _method, value, nameof(Method)); } }

        SubscriptionKind _subKind;
        public SubscriptionKind SubscriptionKind { get { return _subKind; } set { SetIfDifferentVT<SubscriptionKind>(ref _subKind, value, nameof(SubscriptionKind)); } }

        SubscriptionPriorityGroup _priorityGroup;
        public SubscriptionPriorityGroup PriorityGroup { get { return _priorityGroup; } set { SetIfDifferentVT<SubscriptionPriorityGroup>(ref _priorityGroup, value, nameof(PriorityGroup)); } }


        //public PropDoWhenChangedField() : this(null, null, true, null, null, null) {}

        //public PropDoWhenChangedField(Delegate doWhenChangedAction, bool doAfterNotify = false)
        //{
        //    DoWhenChangedAction = doWhenChangedAction;
        //    DoAfterNotify = doAfterNotify;
        //}

        //public Func<object, EventHandler<PcGenEventArgs>> DoWhenGenHandlerGetter { get; }

        //public PropDoWhenChangedField(EventHandler<PcGenEventArgs> doWhenChangedAction, bool doAfterNotify,
        //    bool methodIsLocal, Type declaringType, string fullClassName, string instanceKey, string methodName,
        //    Func<object, EventHandler<PcGenEventArgs>> doWhenChangedActionGetter)
        //{
        //    //DoWhenChangedAction = doWhenChangedAction;
        //    //DoAfterNotify = doAfterNotify;
        //    MethodIsLocal = methodIsLocal;
        //    DeclaringType = declaringType; // ?? throw new ArgumentNullException(nameof(declaringType));
        //    FullClassName = fullClassName; //  ?? throw new ArgumentNullException(nameof(fullClassName));
        //    InstanceKey = instanceKey; //  ?? throw new ArgumentNullException(nameof(instanceKey));
        //    MethodName = methodName; //  ?? throw new ArgumentNullException(nameof(methodName));

        //    DoWhenGenHandlerGetter = doWhenChangedActionGetter;
        //}

        public PropDoWhenChangedField(object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup,
            bool methodIsLocal, Type declaringType, string fullClassName, string instanceKey)
        {
            Target = target; // ?? throw new ArgumentNullException(nameof(target));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            SubscriptionKind = subscriptionKind;
            PriorityGroup = priorityGroup;

            MethodIsLocal = methodIsLocal;
            DeclaringType = declaringType; // ?? throw new ArgumentNullException(nameof(declaringType));
            FullClassName = fullClassName; //  ?? throw new ArgumentNullException(nameof(fullClassName));
            InstanceKey = instanceKey; //  ?? throw new ArgumentNullException(nameof(instanceKey));
            MethodName = method.Name;

            //DoWhenGenHandlerGetter = null;
        }

        public bool Equals(PropDoWhenChangedField other)
        {
            if (other == null) return false;

            if (other.Target == Target && other.Method == Method) return true;

            return false;
        }
    }
}
