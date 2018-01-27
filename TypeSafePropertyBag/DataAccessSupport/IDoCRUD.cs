using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    public interface IDoCRUD<TEntity> : IDisposable
    {
        void Delete(TEntity personToDelete);
        IEnumerable<TEntity> Get();
        IEnumerable<TEntity> Get(int top);
        //IEnumerable<TEntity> Get<TKey>(int start, int count, Func<TEntity, TKey> keySelector);
        void Update(TEntity updatedPerson);

        event EventHandler<EventArgs> DataSourceChanged;

        TEntity GetNewItem();
    }
}