using System;

namespace DRM.PropBag.LocalBinding
{
    public enum LocalBindingMode
    {
        /// <summary>
        /// Combines modes OneWay and OneWayToSource
        /// </summary>
        TwoWay = 0,

        /// <summary>
        /// Updates the binding target (target) property when the binding source (source) changes.
        /// </summary>
        OneWay = 1,

        /// <summary>
        /// The target is updated upon creation of the binding, or as soon as the target becomes available.
        /// </summary>
        OneTime = 2,

        /// <summary>
        /// Updates the source property when the target property changes.
        /// </summary>
        OneWayToSource = 3,

        /// <summary>
        /// Uses the default System.Windows.Data.Binding.Mode value of the binding target.
        /// </summary>
        Default = 4
    }
}
