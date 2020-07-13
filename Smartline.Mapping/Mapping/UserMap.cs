using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Smartline.Mapping.Entity;

namespace Smartline.Mapping.Mapping {
    public class UserMap : ClassMap<User> {
        public UserMap(){
            Id(x => x.UID).GeneratedBy.Guid();
            Map(x => x.UserName);
            Map(x => x.Name);
            Map(x => x.Surname);
            Map(x => x.Patronymic);
            Map(x => x.Info);
            Map(x => x.Secret);
            HasMany(x => x.Tracker)
                .ForeignKeyCascadeOnDelete()
                .Inverse()
                .KeyColumn("UserId");
        }
    }                     
}