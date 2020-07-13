using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class TrackerWrapperCollection : IList<TrackerWrapper>, IDisposable {
        private List<TrackerWrapper> _trackerWrappers;

        public TrackerWrapperCollection() {
            _trackerWrappers = new List<TrackerWrapper>();
        }

        #region IDisposable implementation

        public void Dispose() {
            foreach (TrackerWrapper trackerWrapper in _trackerWrappers) {
                trackerWrapper.Dispose();
            }
        }

        #endregion

        #region IList implementation

        public int IndexOf(TrackerWrapper item) {
            return _trackerWrappers.IndexOf(item);
        }

        public void Insert(int index, TrackerWrapper item) {
            _trackerWrappers.Insert(index, item);
        }

        public void RemoveAt(int index) {
            _trackerWrappers.RemoveAt(index);
        }

        public TrackerWrapper this[int index] {
            get { return _trackerWrappers[index]; }
            set { _trackerWrappers[index] = value; }
        }

        public void Add(TrackerWrapper item) {
            _trackerWrappers.Add(item);
            this.SubscribeTrackerEvent(item);
        }

        public void Clear() {
            _trackerWrappers.Clear();
        }

        public bool Contains(TrackerWrapper item) {
            return _trackerWrappers.Contains(item);
        }

        public void CopyTo(TrackerWrapper[] array, int arrayIndex) {
            _trackerWrappers.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _trackerWrappers.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(TrackerWrapper item) {
            return _trackerWrappers.Remove(item);
        }

        public IEnumerator<TrackerWrapper> GetEnumerator() {
            return _trackerWrappers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _trackerWrappers.GetEnumerator();
        }

        #endregion

        private void SubscribeTrackerEvent(TrackerWrapper wrapper) {
            wrapper.StoppedEvent += x1 => this.Remove(wrapper);
        }
    }
}