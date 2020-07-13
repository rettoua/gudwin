using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;

namespace Smartline.Mapping.Repository {
    public class EntityRepository<T> : IRepository<T> {

        private ISession _session;

        public EntityRepository(ISession session) {
            this._session = session;
        }

        #region IRepository<T> Members

        public void Save(T item) {
            _session.Save(item);
        }

        public List<T> ReadAll() {
            return new List<T>(_session.CreateCriteria(typeof(T)).List<T>());
        }

        public T ReadById(Guid id) {
            return _session.Get<T>(id);
        }

        public void Delete(T item) {
            _session.Delete(item);
        }

        public IQueryable<T> Linq(){
            return _session.Query<T>();
        }

        #endregion
    }
}