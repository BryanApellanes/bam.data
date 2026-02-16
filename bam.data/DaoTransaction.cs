/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Provides transaction-like behavior for Dao operations by tracking inserts, updates, and deletes
    /// and supporting rollback by undoing those operations.
    /// </summary>
    public class DaoTransaction: IDaoTransaction, IDisposable
    {
        /// <summary>
        /// Occurs after the transaction has been committed.
        /// </summary>
        public event EventHandler Committed;

        /// <summary>
        /// Occurs after the transaction has been rolled back.
        /// </summary>
        public event EventHandler RolledBack;

        /// <summary>
        /// Occurs when the transaction is disposed.
        /// </summary>
        public event EventHandler Disposed;

        List<IDao> _toDelete = new List<IDao>();
        List<IDao> _toUndo = new List<IDao>();
        List<IDao> _toUndelete = new List<IDao>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DaoTransaction"/> class, subscribing to Dao commit and delete events.
        /// </summary>
        /// <param name="database">The database to use for the transaction.</param>
        public DaoTransaction(IDatabase database)
        {
            this._db = new Database(database.ServiceProvider.Clone(), database.ConnectionString, database.ConnectionName);
            Dao.BeforeCommitAny += DaoBeforeCommitAny;
            Dao.BeforeDeleteAny += DaoBeforeDeleteAny;
        }

        protected void DaoBeforeCommitAny(IDatabase db, IDao dao)
        {
            if (db == this.Database)
            {
                if (dao.IsNew)
                {
                    _toDelete.Add(dao); // it's being inserted
                }
                else
                {
                    _toUndo.Add(dao); // it's being updated
                }
            }
        }

        protected void DaoBeforeDeleteAny(IDatabase db, IDao dao)
        {
            if (db == this.Database)
            {
                _toUndelete.Add(dao); 
            }
        }

        IDatabase _db;

        /// <summary>
        /// Gets the database associated with this transaction.
        /// </summary>
        public IDatabase Database
        {
            get
            {
                return this._db;
            }
        }

        protected bool WasCommitted { get; set; }

        /// <summary>
        /// Commits the transaction, marking it as completed and preventing rollback on dispose.
        /// </summary>
        public void Commit()
        {
            WasCommitted = true;
            OnCommitted();
        }

        public void Rollback()
        {
            WasCommitted = false;
            foreach (Dao dao in this._toDelete)
            {
                dao.Delete();
            }
            foreach (Dao dao in this._toUndelete)
            {
                dao.Undelete();
            }
            foreach (Dao dao in this._toUndo)
            {
                dao.Undo();
            }

            OnRolledback();
        }

        private void OnCommitted()
        {
            if (Committed != null)
            {
                Committed(this, new EventArgs());
            }
        }

        private void OnRolledback()
        {
            if (RolledBack != null)
            {
                RolledBack(this, new EventArgs());
            }
        }

        private void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, new EventArgs());
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!WasCommitted)
            {
                this.Rollback();
            }

            Dao.BeforeDeleteAny -= DaoBeforeDeleteAny;
            Dao.BeforeCommitAny -= DaoBeforeCommitAny;
            this.OnDisposed();
        }

        #endregion
    }
}
