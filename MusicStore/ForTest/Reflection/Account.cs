using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForTest.Reflection
{
    public class Account
    {
        // Attribute（Field）字段
        private int id;
        private string name;
        public string pwd;

        // Property 属性
        public int ID
        {
            set { id = value; }
            get { return id; }
        }

        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        public string Pwd {
            set { pwd = value; }
            get { return pwd; }
        }

        // Constructor 构造器
        public Account(int id)
        {
            this.ID = id;
        }

        // Method 成员方法
        public string ToJsonString()
        {
            return string.Format(@"{ID:{0},Name:{1},Pwd:{2}}", ID, Name, Pwd);
        }

    }
}
