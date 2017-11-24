using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag
{
    //
    // Summary:
    //     Implements a data structure for describing a property as a path below another
    //     property, or below an owning type.
    public class LocalPropertyPath : IEquatable<LocalPropertyPath>
    {
        public string Path { get; }

        public LocalPropertyPath(string path)
        {
            Path = path;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LocalPropertyPath);
        }

        public bool Equals(LocalPropertyPath other)
        {
            return other != null &&
                   Path == other.Path;
        }

        // TODO: Can this me made to be more efficient?
        public override int GetHashCode()
        {
            return 467214278 + EqualityComparer<string>.Default.GetHashCode(Path);
        }

        public static bool operator ==(LocalPropertyPath path1, LocalPropertyPath path2)
        {
            return EqualityComparer<LocalPropertyPath>.Default.Equals(path1, path2);
        }

        public static bool operator !=(LocalPropertyPath path1, LocalPropertyPath path2)
        {
            return !(path1 == path2);
        }
    }
}





