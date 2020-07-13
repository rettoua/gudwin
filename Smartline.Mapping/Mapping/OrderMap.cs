using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Smartline.Mapping.Entity;

namespace Smartline.Mapping.Mapping {
    public class OrderMap : ClassMap<Order> {
        public OrderMap(){
            Id(x => x.Id);
            Map(x => x.Length);
            Map(x => x.DriverNumber);
            Map(x => x.Tariff);
            Map(x => x.SendTime);
            Map(x => x.Command);
            Map(x => x.TrackerId);
            Map(x => x.Latitude);
            Map(x => x.Longitude);
        }
    }
}
