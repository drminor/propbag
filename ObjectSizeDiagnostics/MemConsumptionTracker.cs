using System;
using System.Diagnostics;

namespace ObjectSizeDiagnostics
{
    public class MemConsumptionTracker
    {
        static TrackerStore _trackerStore = new TrackerStore();

        const string DEFAULT_MSG_PREFIX = "MT";
        const string AMOUNT_REPORT_FORMAT = "##,#";

        string _msgPrefix;
        bool _enabled;
        long _lastKnownTotalMemory;

        #region Constructor

        public MemConsumptionTracker() : this(DEFAULT_MSG_PREFIX, null, true, null) { }

        public MemConsumptionTracker(bool enabled) : this(DEFAULT_MSG_PREFIX, null, enabled, null) { }

        public MemConsumptionTracker(string msgPrefix, string message) : this(msgPrefix, message, true, null) { }

        public MemConsumptionTracker(string msgPrefix, bool enabled) : this(msgPrefix, null, enabled, null) { }

        public MemConsumptionTracker(string msgPrefix, string message, bool enabled) : this(msgPrefix, message, enabled, null) { }

        public MemConsumptionTracker(string msgPrefix, string message, bool enabled, long memUsedSoFar) : this(msgPrefix, message, enabled, (long?)memUsedSoFar) { }

        private MemConsumptionTracker(string msgPrefix, string message, bool enabled, long? memUsedSoFar = null) 
        {
            _msgPrefix = msgPrefix;
            Enabled = enabled;
            UsedSoFar = memUsedSoFar ?? _trackerStore.GetOrAddTrackValue(_msgPrefix, 0);

            if (!Enabled) return;

            WriteMessage(_msgPrefix, message + $" [Used So Far: {UsedSoFar.ToString(AMOUNT_REPORT_FORMAT)}]");
        }

        #endregion

        #region Public Properties

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (value == _enabled) return;

                if (value)
                    _lastKnownTotalMemory = System.GC.GetTotalMemory(true);
                else
                    _lastKnownTotalMemory = 0;

                _enabled = value;
            }
        }

        private long _usedSoFar;
        public long UsedSoFar
        {
            get { return _usedSoFar; }
            set
            {
                _usedSoFar = value;
                _trackerStore.UpdateTrack(_msgPrefix, value);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// If you think there's a chance that other processes may have affected the amount of total memory that the Garbage Collector reports,
        /// then call this to take a measurement, just before performing an action for which you want to determine
        /// the amount of memory used.
        /// </summary>
        /// <returns>The total amount of memory used so far as tracked by this instance of MemConsumptionTracker.</returns>
        public long Measure(string message = null)
        {
            if (!Enabled) return 0;

            GC.WaitForPendingFinalizers();

            _lastKnownTotalMemory = GC.GetTotalMemory(true);
            WriteMessage(_msgPrefix, message);
            return UsedSoFar;
        }

        /// <summary>
        /// Perform this after some action to report the difference in the amount of total memory that the Garbage Collector reports.
        /// </summary>
        /// <param name="message">The message to send to Debug.WriteLine</param>
        /// <returns>The total amount of memory used so far as tracked by this instance of MemConsumptionTracker.</returns>
        public long MeasureAndReport(string nameOfOperation, string message = null)
        {
            if (!Enabled) return 0;

            GC.WaitForPendingFinalizers();

            long curBytes = GC.GetTotalMemory(true);
            long usedSinceLastCheck = curBytes - _lastKnownTotalMemory;

            UsedSoFar += usedSinceLastCheck;

            _lastKnownTotalMemory = curBytes;


            nameOfOperation = RemoveTerminatingPeriod(nameOfOperation);

            Debug.Write($"{_msgPrefix}: {nameOfOperation} used {usedSinceLastCheck.ToString(AMOUNT_REPORT_FORMAT)}; total: {UsedSoFar.ToString(AMOUNT_REPORT_FORMAT)}  ");
            CompleteMsgOutput(message);

            return UsedSoFar;
        }

        // TODO: Implement a push and pop model for tracking 'summary' level totals.

        /// <summary>
        /// Call to display a "summary" level message after a method that includes two or more calls to MeasureAndReport.
        /// </summary>
        /// <param name="previousTotal"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public long Report(long previousTotal, string nameOfOperation, string message = null)
        {
            if (!Enabled) return 0;

            nameOfOperation = RemoveTerminatingPeriod(nameOfOperation);

            Debug.Write($"{_msgPrefix}: ---- {nameOfOperation} used {(UsedSoFar - previousTotal).ToString(AMOUNT_REPORT_FORMAT)}; total: {UsedSoFar.ToString(AMOUNT_REPORT_FORMAT)}  ");
            CompleteMsgOutput(message);

            return UsedSoFar;
        }

        public long CompactAndMeasure(string message = null)
        {
            if (!Enabled) return 0;

            GC.WaitForFullGCComplete();
            return Measure(message);
        }

        public long CompactMeasureAndReport(string nameOfOperation, string message = null)
        {
            if (!Enabled) return 0;

            GC.WaitForFullGCComplete();
            return MeasureAndReport(nameOfOperation, message);
        }

        #endregion

        #region Private Methods

        void WriteMessage(string msgPrefix, string message)
        {
            if (message != null)
            {
                if(_msgPrefix == null)
                {
                    Debug.WriteLine(message);
                }
                else
                {
                    Debug.WriteLine(_msgPrefix + ": " + message);
                }
            }
        }

        void CompleteMsgOutput(string message)
        {
            if (message != null)
            {
                Debug.WriteLine(message);
            }
            else
            {
                Debug.WriteLine(string.Empty);
            }
        }

        string RemoveTerminatingPeriod(string x)
        {
            if (x.TrimEnd().EndsWith("."))
            {
                x = x.TrimEnd().Remove(x.Length - 1);
            }
            return x;
        }

        #endregion
    }
}
