using System;
using System.Windows;

namespace DRM.PropBag.ControlsWPF.Binders
{
    /// <summary>
    /// Concrete implementation of the MyBindingEngineBase abstract class.
    /// No members are overridden.
    /// </summary>
    public class MyBindingEngine : MyBindingEngineBase
    {
        public MyBindingEngine(MyBindingInfo bindingInfo, Type sourceType, BindingTarget bindingTarget, bool useMultiBinding = true, string binderInstanceName = "pbb:Binder")
            : base(bindingTarget, bindingInfo, sourceType, useMultiBinding, binderInstanceName)
        {
        }
    }
}
