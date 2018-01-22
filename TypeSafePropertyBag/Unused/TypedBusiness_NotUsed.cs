using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MVVMApplication.Services
{
    public class TypedBusiness<TTable, TEntity> : IDisposable, IDoCRUD<TEntity> where TEntity : class where TTable : DbSet<TEntity>
    {
        PersonDB _dbContext = null;
        TTable _table;

        public TypedBusiness(PersonDB dbContext, TTable table)
        {
            _dbContext = dbContext;
            _table = table;
        }

        public IEnumerable<TEntity> Get()
        {
            IEnumerable<TEntity> result = _table.ToList();
            return result;
        }

        public IEnumerable<TEntity> Get(int top)
        {
            if(top == -1)
            {
                return Get();
            }
            else
            {
                IEnumerable<TEntity> result = _table.Take(top).ToList();
                return result;
            }
        }

        public IEnumerable<TEntity> Get<TKey>(int start, int count, Func<TEntity, TKey> keySelector)
        {
            if (start < 0) throw new ArgumentException("Start must be greater than, or equal to zero.");
            IEnumerable<TEntity> result = _table.OrderBy(keySelector).Skip(start).Take(count);
            return result;
        }

        public void Delete(TEntity personToDelete)
        {
            //if(personToDelete == null || personToDelete.Id == 0) return;

            //Person foundPerson = _dbContext.Person.Find(new object[] { personToDelete.Id });
            //_dbContext.Person.Remove(foundPerson);
            //_dbContext.SaveChanges();
        }

        public void Update(TEntity updatedPerson)
        {
            //CheckValidations(updatedPerson);
            //if (updatedPerson.Id > 0)
            //{
            //    Person selectedPerson = _dbContext.Person.First(p => p.Id == updatedPerson.Id);
            //    selectedPerson.FirstName = updatedPerson.FirstName;
            //    selectedPerson.LastName = updatedPerson.LastName;
            //    selectedPerson.CityOfResidence = updatedPerson.CityOfResidence;
            //    selectedPerson.Profession = updatedPerson.Profession;
            //}
            //else
            //{
            //    _dbContext.Person.Add(updatedPerson);
            //}

            //_dbContext.SaveChanges();
        }

        //private void CheckValidations(TEntity person)
        //{
        //    if(person == null)
        //    {
        //        throw new ArgumentNullException("Person", "Please select record from Grid or Add New");
        //    }

        //    if (string.IsNullOrEmpty(person.FirstName))
        //    {
        //        throw new ArgumentNullException("First Name", "Please enter FirstName");
        //    }
        //    else if (string.IsNullOrEmpty(person.LastName))
        //    {
        //        throw new ArgumentNullException("Last Name", "Please enter LastName");
        //    }
        //    else if ((int)person.Profession == -1)
        //    {
        //        throw new ArgumentNullException("Profession", "Please enter Profession");
        //    }
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_dbContext != null) _dbContext.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Business() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
