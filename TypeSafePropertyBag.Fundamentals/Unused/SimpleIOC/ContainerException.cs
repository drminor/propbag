using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

/// <remarks>
/// This Simple inversion of control "Container" was copied whole sale from...
/// http://www.siepman.nl/blog/post/2014/02/15/Simple-IoC-container-easy-to-debug-and-fast.aspx
/// http://www.siepman.nl/blog/author/Admin.aspx
/// </remarks>

namespace DRM.TypeSafePropertyBag.Fundamentals.SimpleIOC
{
    [Serializable]
    public class ContainerException<T> : Exception
    {
        private readonly string _message;


        public ContainerException(string message)
        {
            _message = string.Format(message, GetFullTypeName());
        }

        public override string Message
        {
            get { return _message; }
        }

        private string GetFullTypeName()
        {
            return GetFullTypeName(typeof(T));
        }

        /// <summary>
        /// Returns a string of regular and generic names
        /// in the format you would expect
        /// </summary>
        private string GetFullTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.ToString();
            }

            var result = new StringBuilder();
            var parentTypeStrings = type.FullName.Split('`');

            var genericArgumentTypes = type.GetGenericArguments();
            var genericArgumentTypesFullNames = genericArgumentTypes.Select(GetFullTypeName);

            var argumentListString = new StringBuilder();
            foreach (var argumentTypeFullName in genericArgumentTypesFullNames)
            {
                if (argumentListString.Length > 0)
                {
                    argumentListString.AppendFormat(", {0}", argumentTypeFullName);
                }
                else
                {
                    argumentListString.Append(argumentTypeFullName);
                }
            }

            if (argumentListString.Length > 0)
            {
                result.AppendFormat("{0}<{1}>", parentTypeStrings[0], argumentListString);
            }

            return result.ToString();
        }

        public override string ToString()
        {
            return $"{nameof(ContainerException<T>)} of <{typeof(T)}>, message: {_message}.";
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected ContainerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
