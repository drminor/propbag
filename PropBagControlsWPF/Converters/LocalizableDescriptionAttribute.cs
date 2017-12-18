using System;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

///<remarks>
/// This was created by Uwe Keim as referenced here:
/// https://www.codeproject.com/Articles/29495/Binding-and-Using-Friendly-Enums-in-WPF
/// See: https://www.codeproject.com/script/Membership/View.aspx?mid=235 for Uwe Keim's profile.
///</remarks>


namespace DRM.PropBag.ControlsWPF.Converters
{

    
    /// <summary>
    /// Attribute for localization.
    /// <summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
    {
        #region Public methods.
        // ------------------------------------------------------------------

        /// &lt;summary>
        /// Initializes a new instance of the 
        /// &lt;see cref="LocalizableDescriptionAttribute"/> class.
        /// &lt;/summary>
        /// &lt;param name="description">The description.&lt;/param>
        /// &lt;param name="resourcesType">Type of the resources.&lt;/param>
        public LocalizableDescriptionAttribute
        (string description, Type resourcesType) : base(description)
        {
            _resourcesType = resourcesType;
        }

        #endregion

        #region Public properties.

        /// &lt;summary>
        /// Get the string value from the resources.
        /// &lt;/summary>
        /// &lt;value>&lt;/value>
        /// &lt;returns>The description stored in this attribute.&lt;/returns>
        public override string Description
        {
            get
            {
                if (!_isLocalized)
                {
                    ResourceManager resMan =
                         _resourcesType.InvokeMember(
                         @"ResourceManager",
                         BindingFlags.GetProperty | BindingFlags.Static |
                         BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as ResourceManager;

                    CultureInfo culture =
                         _resourcesType.InvokeMember(
                         @"Culture",
                         BindingFlags.GetProperty | BindingFlags.Static |
                         BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as CultureInfo;

                    _isLocalized = true;

                    if (resMan != null)
                    {
                        DescriptionValue =
                             resMan.GetString(DescriptionValue, culture);
                    }
                }

                return DescriptionValue;
            }
        }
        #endregion

        #region Private variables.

        private readonly Type _resourcesType;
        private bool _isLocalized;

        #endregion
    }

}





