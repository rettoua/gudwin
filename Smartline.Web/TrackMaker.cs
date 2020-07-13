using System;
using System.Collections.Generic;
using System.Linq;
using Smartline.Common.Runtime;

namespace Smartline.Web {
    public static class TrackMaker {
        public static GpsTrack MakeTrack(List<Gp> source) {
            GpsTrack gpsTrack = GenerateTrack(source.OrderBy(o => o.SendTime));
            return gpsTrack;
        }

        private static GpsTrack GenerateTrack(IEnumerable<Gp> source) {
            var gpsTrack = new GpsTrack();
            gpsTrack.Load(source);
            return gpsTrack;
        }
    }

    public class GpsTrack {
        public List<ITrackItem> TrackItems { get; private set; }

        internal IEnumerable<Gp> GetGps() {
            foreach (ITrackItem trackItem in TrackItems) {
                if (trackItem.TrackItemEnum == TrackItemEnum.Moving) {
                    foreach (Gp gp in ((MovingTrackItem)trackItem).Items) {
                        yield return gp;
                    }
                } else {
                    yield return ((ParkingTrackItem)trackItem).Item;
                }
            }
        }

        internal void Load(IEnumerable<Gp> source) {
            List<ITrackItem> trackItems = GetTrackItems(source);
            trackItems = ValidateTrackItems(trackItems);
            RoundStartOfMovingItems(trackItems);
            foreach (ITrackItem trackItem in trackItems) {
                ApplySpeedFragmentation(trackItem);
            }
            TrackItems = trackItems;
        }

        private List<ITrackItem> GetTrackItems(IEnumerable<Gp> source) {
            var items = new List<ITrackItem>();
            ITrackItem item = null;
            foreach (Gp gp in source) {
                if (item == null) {
                    item = CreateItem(gp);
                    continue;
                }
                if (!item.Add(gp)) {
                    items.Add(item);
                    item = CreateItem(gp);
                }
            }
            if (!items.Contains(item)) {
                items.Add(item);
            }
            return items;
        }

        private ITrackItem CreateItem(Gp gp) {
            if (gp.IsParking()) {
                return new ParkingTrackItem(gp);
            }
            return new MovingTrackItem(gp);
        }

        private List<ITrackItem> ValidateTrackItems(IEnumerable<ITrackItem> trackItems) {
            bool isDataValid = true;
            var list = new List<ITrackItem>();
            ITrackItem prevItem = null;
            foreach (ITrackItem item in trackItems) {
                if (prevItem == null) {
                    prevItem = item;
                    continue;
                }
                if (Add(prevItem, item)) {
                    isDataValid = false;
                } else {
                    list.Add(prevItem);
                    prevItem = item;
                }
            }
            if (!list.Contains(prevItem)) {
                list.Add(prevItem);
            }
            if (!isDataValid) {
                list.RemoveAll(o => o.MarkToDelete);
                list = ValidateTrackItems(list);
            }
            return list;
        }

        private bool Add(ITrackItem item1, ITrackItem item2) {
            bool isValid = item1.IsValid();
            if (isValid && item2.IsValid()) {
                if (item1.TrackItemEnum == item2.TrackItemEnum) {
                    item1.Add(item2);
                    return true;
                }
                return false;
            }
            if (isValid) {
                item1.Add(item2);
            } else {
                item2.Add(item1);
            }
            return true;
        }

        private void RoundStartOfMovingItems(IEnumerable<ITrackItem> trackItems) {
            ITrackItem lastItem = null;
            foreach (ITrackItem item in trackItems) {
                if (lastItem == null) { lastItem = item; continue; }
                if (item.TrackItemEnum == TrackItemEnum.Moving && lastItem.TrackItemEnum == TrackItemEnum.Parking) {
                    ((MovingTrackItem)item).Round(lastItem);
                }
                lastItem = item;
            }
        }

        private void ApplySpeedFragmentation(ITrackItem trackItem) {

        }
    }

    public interface ITrackItem {
        TrackItemEnum TrackItemEnum { get; }
        bool Add(Gp gp);
        void Add(ITrackItem trackItem);
        bool IsValid();
        bool MarkToDelete { get; set; }
    }

    public class MovingTrackItem : ITrackItem {

        public List<Gp> Items { get; private set; }

        public TrackItemEnum TrackItemEnum { get { return TrackItemEnum.Moving; } }

        public bool MarkToDelete { get; set; }

        public MovingTrackItem(Gp gp) {
            Items = new List<Gp> { gp };
        }

        public bool Add(Gp gp) {
            if (gp.IsParking()) { return false; }
            Items.Add(gp);
            return true;
        }

        public void Add(ITrackItem trackItem) {
            if (trackItem.TrackItemEnum == TrackItemEnum.Moving) {
                Add((MovingTrackItem)trackItem);
            } else {
                Add((ParkingTrackItem)trackItem);
            }
        }

        private void Add(MovingTrackItem movingTrackItem) {
            Items.AddRange(movingTrackItem.Items);
            Items = Items.OrderBy(o => o.SendTime).ToList();
            movingTrackItem.MarkToDelete = true;
        }

        private void Add(ParkingTrackItem parkingTrackItem) {
            Items.Add(parkingTrackItem.Item);
            Items = Items.OrderBy(o => o.SendTime).ToList();
            parkingTrackItem.MarkToDelete = true;
        }

        public bool IsValid() {
            return Items.Count > 1;
        }

        public void Round(ITrackItem trackItem) {
            var parkingItem = (ParkingTrackItem)trackItem;
            Gp gp = parkingItem.Item.Clone();
            if (gp.EndTime != null) {
                gp.SendTime = gp.EndTime.Value;
                gp.EndTime = null;
            }
            gp.Speed = 5;
            Items.Insert(0, gp);
        }
    }

    public class ParkingTrackItem : ITrackItem {
        public Gp Item { get; private set; }

        public TrackItemEnum TrackItemEnum { get { return TrackItemEnum.Parking; } }

        public bool MarkToDelete { get; set; }

        public string Diff {
            get {
                if (Item.EndTime != null) {
                    return (Item.EndTime.Value - Item.SendTime).ToString();
                }
                return string.Empty;
            }
        }

        public string FromStr {
            get { return Item.SendTime.ToString(@"HH\:mm\:ss"); }
        }

        public string ToStr {
            get { return Item.EndTime.HasValue ? Item.EndTime.Value.ToString(@"HH\:mm\:ss") : string.Empty; }
        }

        public ParkingTrackItem(Gp item) {
            Item = item;
        }

        public bool Add(Gp gp) {
            if (gp.IsMoving()) {
                return false;
            }
            Item.EndTime = gp.GetActualTime();
            if (gp.Latitude > 0) {
                Item.Latitude = gp.Latitude;
                Item.Longitude = gp.Longitude;
            }
            return true;
        }

        public bool IsValid() {
            if (!Item.EndTime.HasValue) { return false; }
            double diffInSeconds = (Item.EndTime.Value - Item.SendTime).TotalSeconds;
            return diffInSeconds >= 60;
        }

        public void Add(ITrackItem trackItem) {
            if (trackItem.TrackItemEnum == TrackItemEnum.Moving) {
                Add((MovingTrackItem)trackItem);
            } else {
                Add((ParkingTrackItem)trackItem);
            }
        }

        private void Add(MovingTrackItem movingTrackItem) {
            DateTime maxDate = movingTrackItem.Items.Max(o => o.GetActualTime());
            if (Item.GetActualTime() < maxDate) {
                Item.EndTime = maxDate;
            }
            movingTrackItem.MarkToDelete = true;
        }

        private void Add(ParkingTrackItem pakingTrackItem) {
            if (Item.GetActualTime() < pakingTrackItem.Item.GetActualTime()) {
                Item.EndTime = pakingTrackItem.Item.EndTime;
            }
            pakingTrackItem.MarkToDelete = true;
        }
    }

    public enum TrackItemEnum {
        Moving = 1,
        Parking = 2
    }
}

public static class GpsExtensions {
    public static bool IsParking(this Gp gp) {
        if (gp == null) { throw new ArgumentNullException("gp"); }
        return gp.Speed == 0 && gp.Distance == 0;
    }

    public static bool IsMoving(this Gp gp) {
        return !IsParking(gp);
    }
}