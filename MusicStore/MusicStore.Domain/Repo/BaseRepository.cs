using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Dapper.Contrib.Extensions;
using MusicStore.Domain.Data;

namespace MusicStore.Domain.Repo
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        private IUnitOfWork UnitOfWork { set; get; }

        public BaseRepository(IUnitOfWork uw)
        {
            this.UnitOfWork = uw;
        }

        public virtual T Get(long id)
        {
            return this.UnitOfWork.Connection.GetOne<T>(id);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return this.UnitOfWork.Connection.GetAll<T>();
        }

        public virtual long Create(T entity)
        {
            return this.UnitOfWork.Connection.InsertOne<T>(entity);
        }
        
        public virtual bool Update(T entity)
        {
            return this.UnitOfWork.Connection.Update<T>(entity);
        }
        
        /// <summary>
        ///  eg. Delete(new Entity{Id = 1})
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool Delete(T entity)
        {
            return this.UnitOfWork.Connection.DeleteOne<T>(entity);
        }
    }
}
