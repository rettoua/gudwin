using System;
using System.Collections.Generic;
using System.Linq;

namespace Smartline.Mapping.Repository {
    public interface IRepository<T> {
        void Save(T item);

        T ReadById(Guid id);

        List<T> ReadAll();

        void Delete(T item);

        IQueryable<T> Linq();
    }
}