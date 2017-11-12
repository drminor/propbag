using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    //
    // Summary:
    //     Implements a data structure for describing a property as a path below another
    //     property, or below an owning type.
    public sealed class LocalPropertyPath
    {
        public string Path { get; }
        public Collection<object> PathParameters { get; }

        public LocalPropertyPath(string path, params object[] pathParameters)
        {
            Path = path;
            PathParameters = new Collection<object>(pathParameters);
        }
    }
}





