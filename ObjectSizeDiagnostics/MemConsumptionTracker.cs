using System;
using System.Threading;

namespace ObjectSizeDiagnostics
{
    public class MemConsumptionTracker
    {
        bool _enabled;
        long _lastKnownTotalMemory;

        #region Constructor

        public MemConsumptionTracker() : this(true, 0)
        {
        }

        public MemConsumptionTracker(bool enabled) : this(enabled, 0)
        {
        }

        public MemConsumptionTracker(bool enabled, long memUsedSoFar) 
        {
            if (enabled)
                _lastKnownTotalMemory = System.GC.GetTotalMemory(true);
            else
                _lastKnownTotalMemory = 0;

            _enabled = enabled;
            UsedSoFar = memUsedSoFar;
        }

        #endregion

        #region Public Properties

        public long UsedSoFar { get; private set; }

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
            if (!_enabled) return 0;
            Thread.Sleep(10);

            _lastKnownTotalMemory = GC.GetTotalMemory(true);

            if(message != null)
            {
                System.Diagnostics.Debug.WriteLine($"MT: {message}.");
            }
            return UsedSoFar;
        }

        /// <summary>
        /// Perform this after some action to report the difference in the amount of total memory that the Garbage Collector reports.
        /// </summary>
        /// <param name="message">The message to send to Debug.WriteLine</param>
        /// <returns>The total amount of memory used so far as tracked by this instance of MemConsumptionTracker.</returns>
        public long MeasureAndReport(string nameOfOperation, string message = null)
        {
            if (!_enabled) return 0;

            Thread.Sleep(10);

            long curBytes = GC.GetTotalMemory(true);
            long usedSinceLastCheck = curBytes - _lastKnownTotalMemory;

            UsedSoFar += usedSinceLastCheck;

            _lastKnownTotalMemory = curBytes;

            System.Diagnostics.Debug.Write($"MT: {nameOfOperation} used {usedSinceLastCheck}; Total: {UsedSoFar}");
            System.Diagnostics.Debug.WriteLine(message != null ? $" ({message})." : null);
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
            if (!_enabled) return 0;

            System.Diagnostics.Debug.WriteLine($"--MT: {nameOfOperation} used {UsedSoFar - previousTotal}; Total: {UsedSoFar}");
            System.Diagnostics.Debug.WriteLine(message != null ? $" ({message})." : null);

            return UsedSoFar;
        }

        #endregion
    }
}
