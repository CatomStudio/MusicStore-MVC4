using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ForTest.Helpers
{
    public class LogHelper
    {
        public static void WriteLog(Exception ex, string logPath = "")
        {
            // 路径
            var envPath = Environment.CurrentDirectory;
            var currPath = envPath.Substring(0, envPath.Length - @"bin\Debug".Length);
            logPath = (logPath == "" ? currPath : logPath) + @"Log";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            // 输出
            StreamWriter wt = new StreamWriter(logPath + @"\log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", true);
            wt.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            wt.WriteLine("----------- " + ex.Source);
            wt.WriteLine("问题：" + ex.Message);
            wt.WriteLine(ex.StackTrace);
            wt.WriteLine("=======");
            wt.Close();
        }

        public static void WriteLog(string msg, string logPath = "")
        {
            // 路径
            var envPath = Environment.CurrentDirectory;
            var currPath = envPath.Substring(0, envPath.Length - @"bin\Debug".Length);
            logPath = (logPath == "" ? currPath : logPath) + @"Log";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            // 输出
            StreamWriter wt = new StreamWriter(logPath + @"\log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", true);
            wt.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            wt.WriteLine(msg);
            wt.WriteLine("=======");
            wt.Close();
        }

    }
}
