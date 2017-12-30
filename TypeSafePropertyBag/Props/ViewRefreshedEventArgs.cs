using System;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    ///  Provides data for the DRM.TypeSafePropertyBag.IHaveAView.ViewRefreshed event.
    /// </summary>
    public class ViewRefreshedEventArgs : EventArgs
    {
        string _viewName;

        /// <summary>
        /// Initializes a new instance of the DRM.TypeSafePropertyBag.ViewRefreshedEventArgs class.
        /// </summary>
        /// <param name="viewName">The name of the view that changed, should be null to indicate the default view.</param>
        public ViewRefreshedEventArgs(string viewName) 
        {
            _viewName = viewName;
        }

        public virtual string ViewName { get => _viewName; }

        static public ViewRefreshedEventArgs ForDefaultView => new ViewRefreshedEventArgs(null);
    }
}
