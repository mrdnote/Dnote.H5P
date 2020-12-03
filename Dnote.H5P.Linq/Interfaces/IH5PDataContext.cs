using System.Collections.Generic;
using System.Linq;

namespace Dnote.H5P.NetFW.Linq2Sql.Interfaces
{
    public interface IH5PDataContext
    {
        string ConnectionString { get; }

        bool Changed(object item);

        void Delete<T>(T item) where T : class;

        void DeleteAll<T>(IEnumerable<T> items) where T : class;

        void DiscardPendingChanges();

        void Insert<T>(T item) where T : class;

        IQueryable<T> Repository<T>() where T : class;

        void ResetCommandTimeOut();

        void SetCommandTimeOut(int secs);

        void SubmitChanges(bool suppressErrors = false);
    }
}
