﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.ReferenceEquality;

namespace DRM.PropBag
{
    public class Prop<T> : IProp<T>
    {

        public Prop(T curValue, Action<T,T> doWhenChanged, bool doAfterNotify, IEqualityComparer<T> comparer)
        {
            Value = curValue;
            DoWHenChanged = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public T Value { get; set; }
        public Action<T, T> DoWHenChanged { get; set; }
        public IEqualityComparer<T> Comparer { get; private set; }
        public bool DoAfterNotify { get; set; }

        public bool HasCallBack
        {
            get
            {
                return DoWHenChanged != null;
            }
        }

        public bool CompareTo(T newValue)
        {
            return Comparer.Equals(newValue, Value);
        }

        public bool Compare(T val1, T val2)
        {
            return Comparer.Equals(val1, val2);
        }

    }
}
