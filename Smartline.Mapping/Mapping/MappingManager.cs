using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using NHibernate;
using Configuration = NHibernate.Cfg.Configuration;

namespace Smartline.Mapping.Mapping {
    public class MappingManager {
        private Configuration _config;
        private ISession _session;

        public void Map() {
            try
            {
                string connection = ConfigurationManager.AppSettings["ConnectionString"];
                _config = Fluently.Configure().
                                   Database(
                                       MsSqlConfiguration.MsSql2005
                                           .ConnectionString(connection)
                                               .UseReflectionOptimizer())
                                   .Mappings(m => m.FluentMappings.Add<UserMap>()
                                   .Add<TrackerMap>()
                                   .Add<OrderMap>()
                                   .Add<GpsMap>())
                                   .BuildConfiguration();

                ISessionFactory factory = _config.BuildSessionFactory();
                _session = factory.OpenSession();

            } catch (Exception exception) {

            }
        }

        public void CreateDataBase() {
            try {
                new SchemaExport(_config).Create(true, true);
            } catch (Exception exception) {

            }
        }

        public ISession Session {
            get { return _session; }
        }
    }
}
