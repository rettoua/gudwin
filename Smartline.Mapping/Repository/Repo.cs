using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Smartline.Mapping.Entity;
using Smartline.Mapping.Mapping;
using Remotion.Data.Linq.Parsing.Structure;

namespace Smartline.Mapping.Repository {
    public class Repo {
        private IRepository<User> _users;
        private IRepository<Tracker> _trackers;
        private IRepository<Gps> _gpss;
        private IRepository<Order> _orders;

        private MappingManager _mappingManager;

        ~Repo() {
            if (_mappingManager != null) {
                //_mappingManager.Session.Flush();
                _mappingManager.Session.Close();
            }
        }

        public void Flush() {
            if (_mappingManager.Session != null)
                _mappingManager.Session.Flush();
        }

        public Repo(bool createSchema = false) {
            this.InitializeMapping(createSchema);
            this.InitializeRepository();
        }

        private void InitializeMapping(bool createSchema = false) {
            _mappingManager = new MappingManager();
            _mappingManager.Map();
            if (createSchema)
                this.CreateSchema();
        }

        private void CreateSchema() {
            _mappingManager.CreateDataBase();
        }

        private void InitializeRepository() {
            _users = new EntityRepository<User>(_mappingManager.Session);
            _trackers = new EntityRepository<Tracker>(_mappingManager.Session);
            _gpss = new EntityRepository<Gps>(_mappingManager.Session);
            _orders = new EntityRepository<Order>(_mappingManager.Session);
        }

        public IRepository<Gps> Gpss {
            get { return _gpss; }
        }

        public IRepository<Tracker> Trackers {
            get { return _trackers; }
        }

        public IRepository<User> Users {
            get { return _users; }
        }

        public IRepository<Order> Orders {
            get { return _orders; }
        }
    }
}