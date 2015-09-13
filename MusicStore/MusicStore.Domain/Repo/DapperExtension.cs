using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Dapper;

namespace Dapper.Contrib.Extension
{
    /// <summary>
    ///  反射机制，通过实体类获取数据表信息
    /// </summary>
    public static class SqlMapperExtension
    {
        public static string GetTableName<T>()
        {
            var type = typeof(T);
            return type.GetType().Name;
        }

        public static string GetProperties<T>()
        {

        }

        public static IEnumerable<T> Get<T>(this IDbConnection connection)
        {
            string query = string.Format("select * from {0} oder by ID asc", GetTableName<T>());
            return connection.Query<T>(query);
        }

        public static T Get<T>(this IDbConnection connection, int ID)
        {
            string query = string.Format("select * from {0} where ID = {1} ordery by ID", GetTableName<T>(), ID);
            return connection.Query<T>(query).FirstOrDefault();
        }

        public static void Update<T>(this IDbConnection connection, T entity)
        {
            
        }
    }
}
