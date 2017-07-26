using System;

namespace DRM.Ipnwv
{

    // Summary:
    //     Represents the method that will handle the PropBagLib.INotifyPropertyChangedWithVals.PropertyChangedWithVals
    //     event raised when a property is changed on a component.
    //
    // Parameters:
    //   sender:
    //     The source of the event.
    //
    //   e:
    //     A PropBagLib.PropertyChangedWithValsEventArgs that contains the event
    //     data.

    public delegate void PropertyChangedWithValsHandler(object sender, PropertyChangedWithValsEventArgs e);
}
