using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSizeDiagnostics
{
    public class TrackerStore
    {
        Dictionary<string, long> _tracks;

        public TrackerStore()
        {
            _tracks = new Dictionary<string, long>();
        }

        public long? ResetTrack(string trackName)
        {
            long? result = GetTrackValue(trackName);
            UpdateTrack(trackName, 0);
            return result;
        }

        public void UpdateTrack(string trackName, long value)
        {
            if(_tracks.ContainsKey(trackName))
            {
                _tracks[trackName] = value;
            }
            else
            {
                _tracks.Add(trackName, value);
            }
        }

        public long? GetTrackValue(string trackName)
        {
            if(_tracks.TryGetValue(trackName, out long value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public long GetOrAddTrackValue(string trackName, long newValue = 0)
        {
            if (_tracks.TryGetValue(trackName, out long value))
            {
                return value;
            }
            else
            {
                _tracks.Add(trackName, newValue);
                return newValue;
            }
        }

    }
}
