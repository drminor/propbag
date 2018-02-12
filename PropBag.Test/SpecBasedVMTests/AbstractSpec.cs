using System;
using ObjectSizeDiagnostics;

namespace PropBagLib.Tests.SpecBasedVMTests
{
    public abstract class Specification
    {
        protected string MemTrackerMsgPrefix => "BaseTest MT";
        protected string MemTrackerStartMessage { get; set; }

        protected MemConsumptionTracker BaseMemTracker { get; set; }
        protected IDisposable Destroyer { get; set; }

        protected Specification()
        {
        }

        protected void CallBecause_Of()
        {
            BaseMemTracker.CompactMeasureAndReport("Before Because_Of was called.", "Abstract Spec.");
            Because_Of();
            BaseMemTracker.CompactMeasureAndReport("After Because_Of was called.", "Abstract Spec.");
        }

        protected abstract void Because_Of();

        protected abstract Action EstablishContext();

        protected void CallContextEstablisher()
        {
            BaseMemTracker = new MemConsumptionTracker(MemTrackerMsgPrefix, MemTrackerStartMessage, GetMemTrackerEnabledState());
            BaseMemTracker.CompactMeasureAndReport("Before Context Established.", "Abstract Spec.");
            Action cleanupAction = EstablishContext();
            Destroyer = new ContextCleaner(cleanupAction);
            BaseMemTracker.CompactMeasureAndReport("After Context Established.", "Abstract Spec.");
        }

        protected void CallContextDestroyer()
        {
            BaseMemTracker.CompactMeasureAndReport("Before Context Destroyed.", "Abstract Spec.");
            Destroyer.Dispose();
            BaseMemTracker.CompactMeasureAndReport("After Context Destroyed.", "Abstract Spec.");
        }

        protected bool GetMemTrackerEnabledState()
        {
            bool memTrackerEnabled;

#if DEBUG
            memTrackerEnabled = true;
#else
            memTrackerEnabled = false;
#endif
            return memTrackerEnabled;
        }

        //~Specification()
        //{
        //    Destroyer.Dispose();
        //}

    }
}
