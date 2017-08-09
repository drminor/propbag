using System;

namespace DRM.PropBag
{
    /// <summary>
    /// These are the non-type specific features that every instance of IProp<typeparamref name="T"/> implement.
    /// </summary>
    public interface IProp
    {
        object TypedValueAsObject { get; }
        bool ValueIsDefined { get; }

        /// <summary>
        /// Marks the property as having an undefined value.
        /// </summary>
        /// <returns>True, if the value was defined at the time this call was made.</returns>
        bool SetValueToUndefined();

        bool CallBacksHappenAfterPubEvents { get; }
        bool HasCallBack { get; }
        bool HasChangedWithTValSubscribers { get; }

        void CleanUpTyped();
    }
}
