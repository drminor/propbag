using System;
using System.Collections.Generic;
using System.Linq;

namespace PropBagLib.Tests.BusinessModel
{
    public class Business : IDisposable
    {
        PersonDB _dbContext = null;
        public Business()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory",
             Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            _dbContext = new PersonDB();
        }

        internal List<Person> Get(int top = -1)
        {
            List<Person> result;
            if(top != -1)
            {
                result = _dbContext.Person.Take(top).ToList();
            }
            else
            {
                result = _dbContext.Person.ToList();
            }
            return result;
        }

        internal void Delete(Person personToDelete)
        {
            if(personToDelete == null || personToDelete.Id == 0) return;

            Person foundPerson = _dbContext.Person.Find(new object[] { personToDelete.Id });
            _dbContext.Person.Remove(foundPerson);
            _dbContext.SaveChanges();
        }

        internal void Update(Person updatedPerson)
        {
            CheckValidations(updatedPerson);
            if (updatedPerson.Id > 0)
            {
                Person selectedPerson = _dbContext.Person.First(p => p.Id == updatedPerson.Id);
                selectedPerson.FirstName = updatedPerson.FirstName;
                selectedPerson.LastName = updatedPerson.LastName;
                selectedPerson.CityOfResidence = updatedPerson.CityOfResidence;
                selectedPerson.Profession = updatedPerson.Profession;
            }
            else
            {
                _dbContext.Person.Add(updatedPerson);
            }

            _dbContext.SaveChanges();
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_dbContext != null)
                    {
                        _dbContext.Dispose();
                        _dbContext = null;
                    }
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
