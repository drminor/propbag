using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag
{
    //
    // Summary:
    //     Implements a data structure for describing a property as a path below another
    //     property, or below an owning type.
    public class LocalPropertyPath
    {
        public string Path { get; }

        public LocalPropertyPath(string path)
        {
            Path = path;
        }
    }
}





