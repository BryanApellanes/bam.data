/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using Bam.Net.Incubation;

namespace Bam.Net.Data
{
    public class DaoTransaction: IDaoTransaction, IDisposable
    {
        public event EventHandler Committed;
        public event EventHandler RolledBack;
        public event EventHandler Disposed;

        List<IDao> _toDelete = new List<IDao>();
        List<IDao> _toUndo = new List<IDao>();
        List<IDao> _toUndelete = new List<IDao>();

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
        public IDatabase Database
        {
            get
            {
                return this._db;
            }
        }

        protected bool WasCommitted { get; set; }

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
