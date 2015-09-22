using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ForTest.Reflection;
using ForTest.Helpers;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;

namespace ForTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var type = Type.GetType("ForTest.Reflection.Account");
                var p = type.GetProperties();
                var a = type.GetCustomAttributes();
                foreach (var b in p)
                {
                    W(b);
                }

                IDbConnection conn1 = new MySqlConnection();
                IDbConnection conn2 = new MySqlConnection();
                var type1 = conn1.GetType().TypeHandle;
                var type2 = conn2.GetType().TypeHandle;
                W(conn1);
                W(type2+ "--2");
                W(type1);

               
            }
            catch(Exception e)
            {
                LogHelper.WriteLog(e);
            }
        }

        static void W(object obj)
        {
            System.Console.WriteLine(obj.ToString());
        }
    }
}
