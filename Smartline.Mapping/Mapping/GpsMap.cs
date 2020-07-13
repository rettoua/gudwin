using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Smartline.Mapping.Entity;

namespace Smartline.Mapping.Mapping {
    public class GpsMap : ClassMap<Gps> {
        public GpsMap() {
            Id(x => x.UID).GeneratedBy.Guid();
            Map(x => x.Info);
            Map(x => x.SendTime);
            Map(x => x.Latitude);
            Map(x => x.Longitude);
            Map(x => x.Speed);
            Map(x => x.Direction);
            Map(x => x.HemisphereNS);
            Map(x => x.HemisphereEW);
            Map(x => x.CoordinatesFix);
            Map(x => x.Height);
            Map(x => x.LevelFuel);
            Map(x => x.TrackerId);
        }
    }
}