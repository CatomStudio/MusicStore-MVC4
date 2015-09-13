using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Dapper.Contrib.Extension;
using MusicStore.Domain.Data;

namespace MusicStore.Domain.Repo
{
    class BaseRepository<T> : IRepository<T> where T : class
    {
        public IUnitOfWork UnitOfWork { protected set; get; }

        public BaseRepository(IUnitOfWork uw)
        {
            this.UnitOfWork = uw;
        }

        public virtual IEnumerable<T> Get()
        {
            return this.UnitOfWork.Connection.Get<T>();
        }


    }
}
