#nullable enable
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Dnote.H5P.NetFW.Linq2Sql.Interfaces;

namespace Dnote.H5P.NetFW.Linq2Sql
{
    public class H5PDataContext : IH5PDataContext, IDisposable
    {
        private int? _defaultCommandTimeOut;

        public H5PDataContext(string connectionString)
        {
            Context = new DataClassesDataContext(connectionString);
        }

        public DataClassesDataContext Context { get; }

        public string ConnectionString => Context.Connection.ConnectionString;

        /// <summary>
        /// Gets the repository for the given type of entities
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <returns>The repository of the given type</returns>
        public IQueryable<T> Repository<T>() where T : class
        {
            var table = Context.GetTable<T>();
            return table;
        }

        /// <summary>
        /// Deletes the specified entity from the repository
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <param name="item">The entity to delete</param>
        public void Delete<T>(T item) where T : class
        {
            if (item != null)
            {
                ITable table = Context.GetTable<T>();
                table.DeleteOnSubmit(item);
            }
        }

        public void DeleteAll<T>(IEnumerable<T> items) where T : class
        {
            ITable table = Context.GetTable<T>();
            table.DeleteAllOnSubmit(items);
        }

        public bool Changed(object item)
        {
            return Context.GetChangeSet().Updates.Contains(item);
        }

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <param name="item">The entity to add</param>
        public void Insert<T>(T item) where T : class
        {
            ITable table = Context.GetTable<T>();
            table.InsertOnSubmit(item);
        }

        /// <summary>
        /// Submits the changes.
        /// </summary>
        public void SubmitChanges(bool suppressErrors = false)
        {
            try
            {
                Context.SubmitChanges();
            }
            catch (ChangeConflictException)
            {
                foreach (var objectChangeConflict in Context.ChangeConflicts)
                {
                    objectChangeConflict.Resolve(RefreshMode.OverwriteCurrentValues);
                }
            }
            catch (Exception)
            {
                if (!suppressErrors)
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public void SetCommandTimeOut(int secs)
        {
            if (_defaultCommandTimeOut == null)
            {
                _defaultCommandTimeOut = Context.CommandTimeout;
            }

            Context.CommandTimeout = secs;
        }

        public void ResetCommandTimeOut()
        {
            if (_defaultCommandTimeOut != null)
            {
                Context.CommandTimeout = _defaultCommandTimeOut.Value;
            }
        }

        /// <summary>
        /// Discard all pending changes of current DataContext.
        /// All un-submitted changes, including insert/delete/modify will lost.
        /// </summary>
        public void DiscardPendingChanges()
        {
            RefreshPendingChanges(RefreshMode.OverwriteCurrentValues);

            var changeSet = Context.GetChangeSet();
            //Undo inserts
            foreach (object objToInsert in changeSet.Inserts)
            {
                Context.GetTable(objToInsert.GetType()).DeleteOnSubmit(objToInsert);
            }
            //Undo deletes
            foreach (object objToDelete in changeSet.Deletes)
            {
                Context.GetTable(objToDelete.GetType()).InsertOnSubmit(objToDelete);
            }
        }

        /// <summary>
        ///     Refreshes all pending Delete/Update entity objects of current DataContext according to the specified mode.
        ///     Nothing will do on Pending Insert entity objects.
        /// </summary>
        public void RefreshPendingChanges(RefreshMode refreshMode)
        {
            var changeSet = Context.GetChangeSet();
            Context.Refresh(refreshMode, changeSet.Deletes);
            Context.Refresh(refreshMode, changeSet.Updates);
        }
    }
}
