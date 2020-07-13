using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Smartline.Mapping.Entity;

namespace Smartline.Mapping.Mapping{
    public class TrackerMap : ClassMap<Tracker>{
        public TrackerMap(){
            Id(x => x.UID).GeneratedBy.Guid();
            Map(x => x.Car);
            Map(x => x.Info);
            Map(x => x.TrackerId);
        }
    }
}