using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Threading;
using Dapper;
using System.Collections;
using MusicStore.Domain.Utils;
using MusicStore.Domain.Data;


namespace Dapper.Contrib.Extensions
{
    /// <summary>
    ///  基于Dapper 的 CRUD 扩展。
    /// </summary>
    public static class SqlMapperExtensions
    {
        #region 1. 表、字段、SQL语句 容器
        // 属性容器
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> PropertyDic = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        // 主键容器（PK）
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> PKDic = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        // 需计算字段容器
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        // 可写字段（普通字段 = 所有字段 - 主键 - 需计算字段）
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> WriteableProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        // 查询语句
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        // 插入语句
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> InsertQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        // 更新语句
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> UpdateQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        // 删除语句
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> DeleteQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        // 实体名
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TableNameDic = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        // db适配器容器。键为Connection类型，如MySqlDbConnection
        private static readonly Dictionary<string, ISqlAdapter> AdapterDic = new Dictionary<string, ISqlAdapter>() { { "mysqlconnection", new MySqlAdapter() } };
        // db格式容器
        private static readonly Dictionary<string, SqlEscape> EscapeDic = new Dictionary<string, SqlEscape>() { { "mysqlconnection", new SqlEscape() { LEscape = "`", REscape = "`" } } };
        #endregion

        #region 2. 属性容器的 存取

        /// <summary>
        ///  获取实体类的所有属性。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> PropertyDicCache(Type type)
        {
            IEnumerable<PropertyInfo> pis;
            if (PropertyDic.TryGetValue(type.TypeHandle, out pis))
            {
                return pis;
            }

            var properties = type.GetProperties().Where(IsWriteable).ToArray();
            PropertyDic[type.TypeHandle] = properties;
            return properties;
        }

        /// <summary>
        ///  获取实体类的主键集。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> PKDicCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            // 1. 判断：主键容器中是否已装载了此类（表对应的实体类）的成员信息集
            if (PKDic.TryGetValue(type.TypeHandle, out pi))
            {
                return pi;
            }

            // 2. 从所有属性容器中，找到主键集，装入主键容器
            // 两种找主键方法：1）通过实体主键注解；2）自定义格式设定注解名称。
            // 双管齐下，避免失误。
            var properties = PropertyDicCache(type);
            // 方法一
            var keys = properties.Where(p => p.GetCustomAttributes(true).Any(a => a is PrimaryKeyAttribute)).ToList();

            // 方法二
            if (keys.Count == 0)
            {
                var idProp = properties.Where(p => p.Name.ToLower() == "id" || p.Name.ToLower() == "orgid").FirstOrDefault();
                if (idProp != null)
                {
                    keys.Add(idProp);
                }
            }

            PKDic[type.TypeHandle] = keys;
            return keys;
        }

        private static IEnumerable<PropertyInfo> ComputedPropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            if (ComputedProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi;
            }

            var computedProperties = PropertyDicCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputableAttribute)).ToList();

            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        /// <summary>
        ///  属性集中是否包含可写（一般）属性，即数据表是否有可编辑字段
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static bool IsWriteable(PropertyInfo pi)
        {
            object[] attributes = pi.GetCustomAttributes(typeof(WriteableAttribute), false);
            if (attributes.Length >= 1)
            {
                WriteableAttribute write = (WriteableAttribute)attributes[0];
                return write.canWrite;
            }
            return true; // 默认都可写
        }

        /// <summary>
        ///  返回的是表名。如 Order，而不是 `Order`。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTableName(Type type)
        {
            string name;
            if (!TableNameDic.TryGetValue(type.TypeHandle, out name))
            {
                name = type.Name;
                if (type.IsInterface && name.StartsWith("I"))
                    name = name.Substring(1);
                //NOTE: This as dynamic trick should be able to handle both our own Table-attribute as well as the one in EntityFramework 
                var tableattr = type.GetCustomAttributes(false).Where(attr => attr.GetType().Name == "TableAttribute").SingleOrDefault() as dynamic;
                if (tableattr != null)
                    name = tableattr.Name;
                TableNameDic[type.TypeHandle] = name;
            }
            return name;
        }

        /// <summary>
        ///  根据连接类型，获取对应的DB适配器
        /// </summary>
        /// <param name="connection">DbConnection，如MySqlDbConnection</param>
        /// <returns>如 MySqlAdapter</returns>
        private static ISqlAdapter GetAdapter(IDbConnection connection)
        {
            string name = connection.GetType().Name.ToLower();
            if (!AdapterDic.ContainsKey(name))
                return new MySqlAdapter();
            return AdapterDic[name];
        }

        private static SqlEscape GetEscape(IDbConnection connection)
        {
            string name = connection.GetType().Name.ToLower();
            if (!EscapeDic.ContainsKey(name))
                return null;
            return EscapeDic[name];
        }
        #endregion

        #region 3. SQL 格式器，按实体属性

        /// <summary>
        ///  仅支持单一主键的表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="escape"></param>
        /// <returns></returns>
        private static string GetQueryFormatter(Type type, SqlEscape escape = null)
        {
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            {
                // 1. 是否有主键
                var keys = PKDicCache(type);
                if (keys.Count() > 1)
                    throw new DataException("Get<T> only supports an entity with a single [Key] property");
                if (keys.Count() == 0)
                    throw new DataException("Get<T> only supports en entity with a [Key] property");

                // 2. 限支持单一主键
                var onlyKey = keys.First();

                // 3. 表名
                var tbName = GetTableName(type);

                // 4. sql 格式化
                var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);
                var keyName = escape == null ? onlyKey.Name : string.Format("{0}{1}{2}", escape.LEscape, onlyKey.Name, escape.REscape);
                sql = string.Format(@"select * from {0} where {1} = @{2}", tbNameFormat, keyName, keyName);
                GetQueries[type.TypeHandle] = sql;
            }
            return sql;
        }

        /// <summary>
        ///  插入的SQL语句
        /// </summary>
        /// <param name="type"></param>
        /// <param name="escape"></param>
        /// <returns>插入的sql语句，格式：insert into Tb (c1,...,cN) values (@{c1}, ... , @{cN})</returns>
        private static string InsertQueryFormatter(Type type, SqlEscape escape = null)
        {
            string sql;
            if (!InsertQueries.TryGetValue(type.TypeHandle, out sql))
            {
                // 1. 获取表名
                var tbName = GetTableName(type);

                // 2. 循环，获取普通字段名
                var sbColumnList = new StringBuilder(null);
                var properties = PropertyDicCache(type);
                var keys = PKDicCache(type);
                var computedProperties = ComputedPropertiesCache(type);
                var propertyDicExceptKeyAndComputed = properties.Except(keys.Union(computedProperties));

                for (var i = 0; i < propertyDicExceptKeyAndComputed.Count(); i++)
                {
                    var property = propertyDicExceptKeyAndComputed.ElementAt(i);
                    var keyName = escape == null ? property.Name : string.Format("{0}{1}{2}", escape.LEscape, property.Name, escape.REscape);
                    sbColumnList.AppendFormat("{0}", keyName);
                    if (i < propertyDicExceptKeyAndComputed.Count() - 1)
                        sbColumnList.Append(", ");
                }

                // 3. 循环，将所有普通字段写入sql格式
                var sbParameterList = new StringBuilder(null);
                for (var i = 0; i < propertyDicExceptKeyAndComputed.Count(); i++)
                {
                    var property = propertyDicExceptKeyAndComputed.ElementAt(i);
                    sbParameterList.AppendFormat("@{0}", property.Name);
                    if (i < propertyDicExceptKeyAndComputed.Count() - 1)
                        sbParameterList.Append(", ");
                }

                var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);
                sql = String.Format("insert into {0} ({1}) values ({2})", tbNameFormat, sbColumnList.ToString(), sbParameterList.ToString());
                InsertQueries[type.TypeHandle] = sql;
            }
            return sql;
        }

        /// <summary>
        ///  更新所有一般字段的SQL语句。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="escape"></param>
        /// <returns>修改的 SQL 语句格式器</returns>
        private static string UpdateQueryFormatter(Type type, SqlEscape escape = null)
        {
            string sql;
            if (!UpdateQueries.TryGetValue(type.TypeHandle, out sql))
            {
                // 1. 取主键、表名
                var keys = PKDicCache(type);
                if (!keys.Any())
                    throw new ArgumentException("Entity must have at least one [Key] property");

                var tbName = GetTableName(type);
                var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);

                // 2. 拼接 SQL
                var sb = new StringBuilder();
                sb.AppendFormat("update {0} set ", tbNameFormat);

                var properties = PropertyDicCache(type);
                var computedProperties = ComputedPropertiesCache(type);
                // 得到普通属性
                var nonIdProps = properties.Except(keys.Union(computedProperties));

                if (nonIdProps.Count() == 0)
                    throw new ArgumentException("Entity must have at least one field that can update!");
                // 循环拼接
                for (var i = 0; i < nonIdProps.Count(); i++)
                {
                    var property = nonIdProps.ElementAt(i);
                    var keyName = escape == null ? property.Name : string.Format("{0}{1}{2}", escape.LEscape, property.Name, escape.REscape);
                    sb.AppendFormat("{0} = @{1}", keyName, property.Name);
                    if (i < nonIdProps.Count() - 1)
                        sb.AppendFormat(", ");
                }
                // 主键条件
                sb.Append(" where ");
                for (var i = 0; i < keys.Count(); i++)
                {
                    var property = keys.ElementAt(i);
                    var keyName = escape == null ? property.Name : string.Format("{0}{1}{2}", escape.LEscape, property.Name, escape.REscape);
                    sb.AppendFormat("{0} = @{1}", keyName, property.Name);
                    if (i < PKDic.Count() - 1)
                        sb.AppendFormat(" and ");
                }
                sql = sb.ToString();
                UpdateQueries[type.TypeHandle] = sql;
            }
            return sql;
        }

        /// <summary>
        ///   选择性更新字段的SQL语句格式器。<br>注：此sql无Dic容器。  
        /// </summary>
        /// <param name="entity">所需修改的实体数据</param>
        /// <returns></returns>
        private static string UpdatePartQueryFormatter(Type type, object entity, SqlEscape escape = null)
        {
            var keys = PKDicCache(type);
            if (!keys.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var tbName = GetTableName(type);
            var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);
            var keyNames = keys.Select(p => p.Name.ToLower());

            var sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", tbNameFormat);

            // 获取所有普通字段
            IEnumerable<string> nonIdPropNames;
            if (entity is IEnumerable<KeyValuePair<string, object>>)
            {
                var obj = entity as IEnumerable<KeyValuePair<string, object>>;
                nonIdPropNames = obj.Where(p => !keyNames.Contains(p.Key.ToLower())).Select(p => p.Key);
            }
            else
            {
                var properties = entity.GetType().GetProperties().Where(IsWriteable).ToArray();
                nonIdPropNames = properties.Where(p => !keyNames.Contains(p.Name.ToLower())).Select(p => p.Name);
            }

            // 拼接所有普通字段
            for (var i = 0; i < nonIdPropNames.Count(); i++)
            {
                var property = nonIdPropNames.ElementAt(i);
                var keyName = escape == null ? property : string.Format("{0}{1}{2}", escape.LEscape, property, escape.REscape);
                sb.AppendFormat("{0} = @{1}", keyName, property);
                if (i < nonIdPropNames.Count() - 1)
                    sb.AppendFormat(", ");
            }
            sb.Append(" where ");

            // 拼接条件字段（主键）
            for (var i = 0; i < keys.Count(); i++)
            {
                var property = keys.ElementAt(i);
                var keyName = escape == null ? property.Name : string.Format("{0}{1}{2}", escape.LEscape, property.Name, escape.REscape);
                sb.AppendFormat("{0} = @{1}", keyName, property.Name);
                if (i < keys.Count() - 1)
                    sb.AppendFormat(" and ");
            }
            return sb.ToString();
        }

        /// <summary>
        ///  按条件选择性更新数据的 SQL 语句格式器。<b>可更新多条</b>
        /// </summary>
        /// <param name="source">更新字段</param>
        /// <param name="condition">条件字段（未必主键）</param>
        /// <param name="escape"></param>
        /// <returns>修改的SQL格式器。格式：update Tb set c1=@c1,c2=@c2, ... ,cN=@cN</returns>
        private static string UpdatePartQueryFormatter(Type type, IDictionary<string, object> source, IDictionary<string, object> condition, SqlEscape escape = null)
        {
            var tbName = GetTableName(type);
            var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);

            var sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", tbNameFormat);

            // 拼接修改字段
            IEnumerable<string> nonIdPropNames = source.Select(p => p.Key);
            for (var i = 0; i < nonIdPropNames.Count(); i++)
            {
                var property = nonIdPropNames.ElementAt(i);
                var keyName = escape == null ? property : string.Format("{0}{1}{2}", escape.LEscape, property, escape.REscape);
                sb.AppendFormat("{0} = @{1}", keyName, property);
                if (i < nonIdPropNames.Count() - 1)
                    sb.AppendFormat(", ");
            }
            sb.Append(" where ");

            // 拼接条件字段
            var keyNames = condition.Select(p => p.Key).ToList();
            for (var i = 0; i < keyNames.Count(); i++)
            {
                var property = keyNames.ElementAt(i);
                var keyName = escape == null ? property : string.Format("{0}{1}{2}", escape.LEscape, property, escape.REscape);

                if (source.ContainsKey(property))
                {
                    string rename = property + DateTime.Now.Ticks;
                    condition.Add(rename, condition[property]);
                    condition.Remove(property);
                    property = rename;
                }

                sb.AppendFormat("{0} = @{1}", keyName, property);
                if (i < keyNames.Count() - 1)
                    sb.AppendFormat(" and ");
            }

            return sb.ToString();
        }

        /// <summary>
        ///  删除的 SQL 格式器。
        /// </summary>
        /// <returns></returns>
        private static string DeleteQueryFormatter(Type type, SqlEscape escape = null)
        {
            string sql;
            if (!DeleteQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var keys = PKDicCache(type);
                if (!keys.Any())
                    throw new ArgumentException("Entity must have at least one [Key] property");

                var sb = new StringBuilder();
                var tbName = GetTableName(type);
                var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);
                sb.Append(string.Format(@"delete * from {0} where ", tbNameFormat));

                // 拼接条件字段（主键）
                for (var i = 0; i < keys.Count(); i++)
                {
                    var property = keys.ElementAt(i);
                    var keyName = escape == null ? property.Name : string.Format("{0}{1}{2}", escape.LEscape, property.Name, escape.REscape);
                    sb.AppendFormat("{0} = @{1}", keyName, property.Name);
                    if (i < keys.Count() - 1)
                        sb.AppendFormat(" and ");
                }
                sql = sb.ToString();
                DeleteQueries[type.TypeHandle] = sql;
            }
            return sql;
        }
        #endregion

        #region 4. CRUD 的映射实现机制，System.Data.IDBConnection 的扩展方法。原理：dapper 的参数化传参@property.
        /// <summary>
        ///  获取一条数据。
        /// </summary>
        /// <typeparam name="T">所查实体（表名）</typeparam>
        /// <param name="connection"></param>
        /// <param name="id">所查元素的主键值</param>
        /// <returns>所取的对象。</returns>
        public static T GetOne<T>(this IDbConnection connection, dynamic keyValue, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            T obj = null;
            var type = typeof(T);

            // 1. 构造查询语句
            string querySQL = GetQueryFormatter(type);
            // 此处改成 Dictionary 会有什么不同？
            string key = PKDicCache(type).First().Name;
            var dynParms = new DynamicParameters();
            dynParms.Add("@" + key, keyValue);

            // 2. 返回查询结果
            if (!type.IsInterface)
                return connection.Query<T>(querySQL, dynParms, transaction: transaction, commandTimeout: commandTimeout).FirstOrDefault();

            // 为什么转型为 Dictionary？
            var res = connection.Query(querySQL, dynParms).FirstOrDefault() as IDictionary<string, object>;

            if (res == null)
                return (T)((object)null);

            obj = ProxyGenerator.GetInterfaceProxy<T>();

            foreach (var property in PropertyDicCache(type))
            {
                var val = res[property.Name];
                property.SetValue(obj, val, null);
            }

            ((IProxy)obj).IsDirty = false;   //reset change tracking and return
            return obj;
        }

        /// <summary>
        ///  查询所有结果。此方法未使用 SQL 格式器。
        /// </summary>
        /// <typeparam name="T">实体类型（表名）</typeparam>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var cacheType = typeof(List<T>);

            // 1. 获取 SQL
            string sql;
            if (!GetQueries.TryGetValue(cacheType.TypeHandle, out sql))
            {
                var keys = PKDicCache(type);
                if (keys.Count() > 1)
                    throw new DataException("Get<T> only supports an entity with a single [Key] property");
                if (!keys.Any())
                    throw new DataException("Get<T> only supports en entity with a [Key] property");

                var tbName = GetTableName(type);
                var escape = GetEscape(connection);
                var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);
                // TODO: query information schema and only select fields that are both in information schema and underlying class / interface 
                sql = string.Format(@"select * from {0}", tbNameFormat);
                GetQueries[cacheType.TypeHandle] = sql;
            }

            // 2. 返回查询结果集
            if (!type.IsInterface)
                return connection.Query<T>(sql, null, transaction, commandTimeout: commandTimeout);

            var result = connection.Query(sql);
            var list = new List<T>();
            foreach (IDictionary<string, object> res in result)
            {
                var obj = ProxyGenerator.GetInterfaceProxy<T>();
                foreach (var property in PropertyDicCache(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, val, null);
                }
                ((IProxy)obj).IsDirty = false;   //reset change tracking and return
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        ///  将单个实体对象的各个属性写入对应表的各个字段。
        /// </summary>
        /// <param name="entityToInsert">所需插入的实体对象</param>
        /// <returns>写入数据所对应的ID</returns>
        public static long InsertOne<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            ISqlAdapter adapter = GetAdapter(connection);
            SqlEscape escape = GetEscape(connection);
            var cmd = InsertQueryFormatter(typeof(T), escape);
            return connection.ExecuteScalar<long>(string.Format("{0};{1}", cmd, adapter.Last_Insert_ID()), entityToInsert, transaction: transaction, commandTimeout: commandTimeout);
        }

        /// <summary>
        ///  批量写入实体对象。
        /// </summary>
        /// <typeparam name="T">实体类型（表名）</typeparam>
        /// <param name="entitiesToInsert">所需插入的实体集</param>
        /// <param name="transaction"></param>
        /// <returns>所写入的数据的条数</returns>
        public static int InsertSome<T>(this IDbConnection connection, IEnumerable<T> entitiesToInsert, IDbTransaction transaction = null) where T : class
        {
            SqlEscape escape = GetEscape(connection);
            var cmd = InsertQueryFormatter(typeof(T), escape);
            return connection.Execute(cmd, entitiesToInsert, transaction);
        }

        /// <summary>
        ///  按实体，修改一条表数据。
        /// </summary>
        /// <typeparam name="T">实体类型（表名）</typeparam>
        /// <param name="entityToUpdate">实体对象（修改数据载体）</param>
        /// <returns>是否修改成功</returns>
        public static bool Update<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var proxy = entityToUpdate as IProxy;
            if (proxy != null)
            {
                if (!proxy.IsDirty) return false;
            }
            var type = typeof(T);
            SqlEscape escape = GetEscape(connection);
            var cmd = UpdateQueryFormatter(type, escape);
            var updated = connection.Execute(cmd, entityToUpdate, commandTimeout: commandTimeout, transaction: transaction);
            return updated > 0;
        }

        /// <summary>
        ///  按实体更新。<b>更新一条</b>
        /// </summary>
        /// <typeparam name="T">实体类型（表名）</typeparam>
        /// <param name="entityToUpdate">更新的实体对象</param>
        /// <returns>是否更新成功</returns>
        public static bool UpdatePart<T>(this IDbConnection connection, object entityToUpdate, IDbTransaction transaction = null) where T : class
        {
            var type = typeof(T);
            SqlEscape escape = GetEscape(connection);
            var cmd = UpdatePartQueryFormatter(type, entityToUpdate, escape);
            return connection.Execute(cmd, entityToUpdate, transaction) > 0;
        }

        /// <summary>
        ///  按主键条件，选择性更新字段。<b>修改一条或一组。</b>
        /// </summary>
        /// <typeparam name="T">实体类型（表名）</typeparam>
        /// <param name="source">更新字段</param>
        /// <param name="condition">条件（主键）</param>
        /// <returns>更新的条数</returns>
        public static int UpdatePart<T>(this IDbConnection connection, IDictionary<string, object> source, IDictionary<string, object> condition, IDbTransaction transaction = null) where T : class
        {
            var type = typeof(T);
            SqlEscape escape = GetEscape(connection);
            var cmd = UpdatePartQueryFormatter(type, source, condition, escape);
            return connection.Execute(cmd, source.Union(condition), transaction);
        }

        /// <summary>
        ///  按实体对象删除一条数据。
        /// </summary>
        /// <returns>是否删除成功</returns>
        public static bool DeleteOne<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            ISqlAdapter adapter = GetAdapter(connection);
            SqlEscape escape = GetEscape(connection);
            var cmd = DeleteQueryFormatter(typeof(T), escape);
            var deleted = connection.Execute(cmd, entityToDelete, transaction: transaction, commandTimeout: commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        ///  删除表内所有数据。
        /// </summary>
        /// <returns>删除的数量</returns>
        public static int DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var tbName = GetTableName(type);
            var escape = GetEscape(connection);
            var tbNameFormat = escape == null ? tbName : string.Format("{0}{1}{2}", escape.LEscape, tbName, escape.REscape);
            var statement = String.Format("delete from {0}", tbNameFormat);
            return connection.Execute(statement, null, transaction: transaction, commandTimeout: commandTimeout);
        }
        #endregion

        #region 5. 反射处理的代理类
        private interface IProxy
        {
            bool IsDirty { get; set; }
        }

        class ProxyGenerator
        {
            // 实体实例容器
            private static readonly Dictionary<Type, object> TypeCache = new Dictionary<Type, object>();

            /// <summary>
            ///  获取动态创建的指定名称的程序集
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private static AssemblyBuilder GetAsmBuilder(string name)
            {
                var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(new AssemblyName { Name = name },
                    AssemblyBuilderAccess.Run);       //NOTE: to save, use RunAndSave

                return assemblyBuilder;
            }

            /// <summary>
            ///  获取实体类的实例对象。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static T GetInterfaceProxy<T>()
            {
                Type typeOfT = typeof(T);

                object k;
                if (TypeCache.TryGetValue(typeOfT, out k))
                {
                    return (T)k;
                }

                // 反射，构造
                var assemblyBuilder = GetAsmBuilder(typeOfT.Name);

                var moduleBuilder = assemblyBuilder.DefineDynamicModule("SqlMapperExtensions." + typeOfT.Name); //NOTE: to save, add "asdasd.dll" parameter

                var interfaceType = typeof(Dapper.Contrib.Extensions.SqlMapperExtensions.IProxy);

                var typeBuilder = moduleBuilder.DefineType(typeOfT.Name + "_" + Guid.NewGuid(), TypeAttributes.Public | TypeAttributes.Class);

                typeBuilder.AddInterfaceImplementation(typeOfT);
                typeBuilder.AddInterfaceImplementation(interfaceType);

                //create our _isDirty field, which implements IProxy
                var setIsDirtyMethod = CreateIsDirtyProperty(typeBuilder);

                // Generate a field for each property, which implements the T
                foreach (var property in typeof(T).GetProperties())
                {
                    var isId = property.GetCustomAttributes(true).Any(a => a is PrimaryKeyAttribute);
                    CreateProperty<T>(typeBuilder, property.Name, property.PropertyType, setIsDirtyMethod, isId);
                }

                var generatedType = typeBuilder.CreateType();

                //assemblyBuilder.Save(name + ".dll");  //NOTE: to save, uncomment

                var generatedObject = Activator.CreateInstance(generatedType);

                TypeCache.Add(typeOfT, generatedObject);
                return (T)generatedObject;
            }

            /// <summary>
            ///  ???
            /// </summary>
            /// <param name="typeBuilder"></param>
            /// <returns></returns>
            private static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
            {
                var propType = typeof(bool);
                var field = typeBuilder.DefineField("_" + "IsDirty", propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty("IsDirty",
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new Type[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                    MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + "IsDirty",
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);
                var currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);
                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + "IsDirty",
                                             getSetAttr,
                                             null,
                                             new Type[] { propType });
                var currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ret);

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(Dapper.Contrib.Extensions.SqlMapperExtensions.IProxy).GetMethod("get_" + "IsDirty");
                var setMethod = typeof(Dapper.Contrib.Extensions.SqlMapperExtensions.IProxy).GetMethod("set_" + "IsDirty");
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);

                return currSetPropMthdBldr;
            }

            /// <summary>
            ///  通过Emit，动态创建实体属性
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="typeBuilder"></param>
            /// <param name="propertyName"></param>
            /// <param name="propType"></param>
            /// <param name="setIsDirtyMethod"></param>
            /// <param name="isIdentity"></param>
            private static void CreateProperty<T>(TypeBuilder typeBuilder, string propertyName, Type propType, MethodInfo setIsDirtyMethod, bool isIdentity)
            {
                //Define the field and the property 
                var field = typeBuilder.DefineField("_" + propertyName, propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(propertyName,
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new Type[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual |
                                                    MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName,
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);

                var currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);

                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                                             getSetAttr,
                                             null,
                                             new Type[] { propType });

                //store value in private field and set the isdirty flag
                var currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldc_I4_1);
                currSetIL.Emit(OpCodes.Call, setIsDirtyMethod);
                currSetIL.Emit(OpCodes.Ret);

                //TODO: Should copy all attributes defined by the interface?
                if (isIdentity)
                {
                    var keyAttribute = typeof(PrimaryKeyAttribute);
                    var myConstructorInfo = keyAttribute.GetConstructor(new Type[] { });
                    var attributeBuilder = new CustomAttributeBuilder(myConstructorInfo, new object[] { });
                    property.SetCustomAttribute(attributeBuilder);
                }

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(T).GetMethod("get_" + propertyName);
                var setMethod = typeof(T).GetMethod("set_" + propertyName);
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);
            }

        }
        #endregion
    }

    /// <summary>
    ///  查询管理器。
    /// </summary>
    public class QueryManager
    {
        public IUnitOfWork UnitOfWork { get; private set; }

        public QueryManager(IUnitOfWork uw)
        {
            this.UnitOfWork = uw;
        }

        public IEnumerable<T> GetList<T>(string sql, string countSql, out long total)
        {
            total = this.UnitOfWork.Connection.ExecuteScalar<long>(countSql);
            return GetList<T>(sql);
        }

        public IEnumerable<T> GetList<T>(string sql)
        {
            return this.UnitOfWork.Connection.Query<T>(sql);
        }

        public int GetExecuteQuery(string sql)
        {
            return this.UnitOfWork.Connection.Execute(sql);
        }

        public int GetExecuteQuery(string sql, object entity)
        {
            return this.UnitOfWork.Connection.Execute(sql, entity);
        }

        public T GetScalar<T>(string sql)
        {
            return this.UnitOfWork.Connection.ExecuteScalar<T>(sql);
        }

        public T GetScalar<T>(string sql, object entity)
        {
            return this.UnitOfWork.Connection.ExecuteScalar<T>(sql, entity);
        }

        public DataTable GetDataTable(string sql)
        {
            return GetDataTable(UnitOfWork.Connection.ExecuteReader(sql));
        }

        private DataTable GetDataTable(IDataReader dataReader)
        {
            using (dataReader)
            {
                //把那个DataReader转化成DataTable。
                DataTable datatable = new DataTable();
                DataTable schemaTable = dataReader.GetSchemaTable();
                //动态添加列
                try
                {
                    foreach (DataRow myRow in schemaTable.Rows)
                    {
                        DataColumn myDataColumn = new DataColumn();
                        myDataColumn.DataType = myRow[11] as Type;
                        var columnName = myRow[0].ToString();
                        myDataColumn.ColumnName = datatable.Columns.Contains(columnName) ?
                            string.Format("{0}.{1}", myRow[10].ToString().ToLower(), columnName) : columnName;
                        myDataColumn.AllowDBNull = Convert.ToBoolean(myRow[12]);
                        datatable.Columns.Add(myDataColumn);
                    }
                    //添加数据
                    while (dataReader.Read())
                    {
                        DataRow myDataRow = datatable.NewRow();
                        for (int i = 0; i < schemaTable.Rows.Count; i++)
                        {
                            //myDataRow[i] = dataReader[i];
                            Type type = dataReader[i].GetType();
                            switch (type.Name)
                            {
                                case "String":
                                    myDataRow[i] = (string)dataReader[i];
                                    break;
                                case "Int16":
                                    myDataRow[i] = datatable.Columns[i].AllowDBNull ? (short?)dataReader[i] : (short)dataReader[i];
                                    break;
                                case "Int32":
                                    myDataRow[i] = datatable.Columns[i].AllowDBNull ? (int?)dataReader[i] : (int)dataReader[i];
                                    break;
                                case "Int64":
                                    myDataRow[i] = datatable.Columns[i].AllowDBNull ? (long?)dataReader[i] : (long)dataReader[i];
                                    break;
                                case "DateTime":
                                    myDataRow[i] = datatable.Columns[i].AllowDBNull ? (DateTime?)dataReader[i] : (DateTime)dataReader[i];
                                    break;
                                case "Decimal":
                                    myDataRow[i] = datatable.Columns[i].AllowDBNull ? (decimal?)dataReader[i] : (decimal)dataReader[i];
                                    break;
                                case "Char":
                                    myDataRow[i] = datatable.Columns[i].AllowDBNull ? (char?)dataReader[i] : (char)dataReader[i];
                                    break;
                                case "Double":
                                    myDataRow[i] = datatable.Columns[i].AllowDBNull ? (double?)dataReader[i] : (double)dataReader[i];
                                    break;
                                default:
                                    myDataRow[i] = dataReader[i];
                                    break;
                            }
                        }
                        datatable.Rows.Add(myDataRow);
                        myDataRow = null;
                    }
                    schemaTable = null;
                    return datatable;
                }
                catch (Exception ex)
                {
                    throw new Exception("转换出错出错!", ex);
                }
            }
        }

        public IEnumerable<T> GetList<T>(string sql, object entity)
        {
            return this.UnitOfWork.Connection.Query<T>(sql, entity);
        }

        public DataTable GetDataTable(string sql, object entity)
        {
            return GetDataTable(UnitOfWork.Connection.ExecuteReader(sql, entity));
        }
    }


    #region 属性校验工具类。3 类属性：主键、可写（普通）、计算（如其他属性相加所得）
    // 表属性
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        private string TbName { set; get; }
        public TableAttribute(string TbName)
        {
            this.TbName = TbName;
        }
    }

    // 主键属性
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        public bool isPK { set; get; }

        public PrimaryKeyAttribute(bool isPK)
        {
            this.isPK = isPK;
        }
    }

    // 可写属性
    [AttributeUsage(AttributeTargets.Property)]
    public class WriteableAttribute : Attribute
    {
        public bool canWrite { get; private set; }

        public WriteableAttribute(bool canWrite)
        {
            this.canWrite = canWrite;
        }
    }

    // 可算属性（如int、long、decimal等）
    [AttributeUsage(AttributeTargets.Property)]
    public class ComputableAttribute : Attribute
    {
        public bool canCompute { set; get; }

        public ComputableAttribute(bool canCompute)
        {
            this.canCompute = canCompute;
        }
    }

    #endregion

    #region 数据库适配器

    /// <summary>
    ///  SQL 字段书写时需要使用的特殊字符。<b>如 MySQL 单独使用 Order 字段会报错，需要改用 `Order`。</b>
    /// </summary>
    public class SqlEscape
    {
        public string LEscape;
        public string REscape;
    }

    /// <summary>
    ///  数据库适配器
    /// </summary>
    public interface ISqlAdapter
    {
        string Last_Insert_ID();
    }

    public class MySqlAdapter : ISqlAdapter
    {
        public string Last_Insert_ID()
        {
            return @"select Last_Insert_ID() id";
        }
    }

    public class MsSqlAdapter : ISqlAdapter
    {
        public string Last_Insert_ID()
        {
            return @"select SCOPE_IDENTITY() as id";
        }
    }

    #endregion

}
