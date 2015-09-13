using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MusicStore.Domain.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        public IDbConnection Connection { private set; get; }

        public UnitOfWork(string connString)
        {
            this.Connection = new MySqlConnection(connString);
        }

        // 打开连接
        public void Open()
        {
            if (Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
        }

        public void Close()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                this.Connection.Close();
            }
        }

        public void Dispose()
        {
            this.Close();
            this.Connection.Dispose();
        }
    }
}
