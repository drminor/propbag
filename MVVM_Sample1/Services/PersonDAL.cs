using DRM.TypeSafePropertyBag.DataAccessSupport;
using MVVMApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MVVMApplication.Services
{
    public class PersonDAL : IDoCRUD<Person>
    {
        #region Constructors

        public PersonDAL(PersonDB dbContext)
        {
            DbContext = dbContext;
        }

        public PersonDAL()
        {
            DbContext = new PersonDB();
        }

        #endregion

        public event EventHandler<EventArgs> DataSourceChanged;

        public PersonDB DbContext { get; }

        public IQueryable<Person> All()
        {
            IQueryable<Person> test = DbContext.Person.AsQueryable<Person>();
            return test;
        }

        public IEnumerable<Person> Get()
        {
            IEnumerable<Person> result = DbContext.Person.AsEnumerable();
            return result;
        }

        public IEnumerable<Person> Get(int top)
        {
            if(top == -1)
            {
                return Get();
            }
            else
            {
                IEnumerable<Person> result = DbContext.Person.Take(top).ToList();
                return result;
            }
        }

        public IEnumerable<Person> Get<TKey>(int start, int count, Func<Person, TKey> keySelector)
        {
            if (start < 0) throw new ArgumentException("Start must be greater than, or equal to zero.");
            IEnumerable<Person> result = DbContext.Person.OrderBy(keySelector).Skip(start).Take(count);
            return result;
        }

        public void Delete(Person personToDelete)
        {
            if(personToDelete == null || personToDelete.Id == 0) return;

            Person foundPerson = DbContext.Person.Find(new object[] { personToDelete.Id });
            DbContext.Person.Remove(foundPerson);
            DbContext.SaveChanges();
        }

        public void Update(Person updatedPerson)
        {
            CheckValidations(updatedPerson);
            if (updatedPerson.Id > 0)
            {
                Person selectedPerson = DbContext.Person.First(p => p.Id == updatedPerson.Id);

                selectedPerson.FirstName = updatedPerson.FirstName;
                selectedPerson.LastName = updatedPerson.LastName;
                selectedPerson.CityOfResidence = updatedPerson.CityOfResidence;
                selectedPerson.Profession = updatedPerson.Profession;
            }
            else
            {
                DbContext.Person.Add(updatedPerson);
            }

            DbContext.SaveChanges();
        }

        public Person GetNewItem()
        {
            Person result = new Person();
            return result;
        }

        private void CheckValidations(Person person)
        {
            if(person == null)
            {
                throw new ArgumentNullException("Person", "Please select record from Grid or Add New");
            }

            if (string.IsNullOrEmpty(person.FirstName))
            {
                throw new ArgumentNullException("First Name", "Please enter FirstName");
            }
            else if (string.IsNullOrEmpty(person.LastName))
            {
                throw new ArgumentNullException("Last Name", "Please enter LastName");
            }
            else if ((int)person.Profession == -1)
            {
                throw new ArgumentNullException("Profession", "Please enter Profession");
            }
        }

        private void OnDataSourceChanged(object sender, EventArgs e)
        {
            Interlocked.CompareExchange(ref DataSourceChanged, null, null)?.Invoke(sender, e);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (DbContext != null) DbContext.Dispose();
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
