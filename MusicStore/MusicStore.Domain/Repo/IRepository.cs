﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Domain.Repo
{
    interface IRepository<T> where T : class
    {
        /// <summary>
        ///  获取所有Entity
        /// </summary>
        /// <returns>实体集</returns>
        IEnumerable<T> Get();

        /// <summary>
        ///  根据ID查询实体
        /// </summary>
        /// <param name="ID">实体ID</param>
        /// <returns>实体</returns>
        T Get(int ID);

        /// <summary>
        ///  新建一个实体
        /// </summary>
        /// <param name="entity">新建的实体</param>
        /// <returns>新建后的ID</returns>
        int Create(T entity);

        /// <summary>
        ///  修改一个实体
        /// </summary>
        /// <param name="entity">实体</param>
        void Update(T entity);

        /// <summary>
        ///  删除实体
        /// </summary>
        /// <param name="ID">实体ID</param>
        void Delete(int ID);
    }
}