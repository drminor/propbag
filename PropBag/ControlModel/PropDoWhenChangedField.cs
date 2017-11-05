using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ControlModel 
{
    public class PropDoWhenChangedField : NotifyPropertyChangedBase, IEquatable<PropDoWhenChangedField>
    {
        Delegate _dwc;
        public Delegate DoWhenChangedAction { get { return _dwc; } set { SetIfDifferentDelegate<Delegate>(ref _dwc, value, nameof(DoWhenChangedAction)); } }

        bool _dan;
        public bool DoAfterNotify { get { return _dan; } set { SetIfDifferent<bool>(ref _dan, value, nameof(DoAfterNotify)); } }

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

        public PropDoWhenChangedField() : this(null, false, true, null, null, null, null, null) {}

        //public PropDoWhenChangedField(Delegate doWhenChangedAction, bool doAfterNotify = false)
        //{
        //    DoWhenChangedAction = doWhenChangedAction;
        //    DoAfterNotify = doAfterNotify;
        //}

        public Func<object, Delegate> DoWhenActionGetter { get; }

        public PropDoWhenChangedField(Delegate doWhenChangedAction, bool doAfterNotify, bool methodIsLocal,
            Type declaringType, string fullClassName, string instanceKey, string methodName,
            Func<object, Delegate> doWhenChangedActionGetter)
        {
            DoWhenChangedAction = doWhenChangedAction;
            DoAfterNotify = doAfterNotify;
            MethodIsLocal = methodIsLocal;
            DeclaringType = declaringType; // ?? throw new ArgumentNullException(nameof(declaringType));
            FullClassName = fullClassName; //  ?? throw new ArgumentNullException(nameof(fullClassName));
            InstanceKey = instanceKey; //  ?? throw new ArgumentNullException(nameof(instanceKey));
            MethodName = methodName; //  ?? throw new ArgumentNullException(nameof(methodName));

            DoWhenActionGetter = doWhenChangedActionGetter;
        }



        public bool Equals(PropDoWhenChangedField other)
        {
            if (other == null) return false;

            if (other.DoAfterNotify == DoAfterNotify && other.DoWhenChangedAction == DoWhenChangedAction) return true;

            return false;
        }
    }
}
