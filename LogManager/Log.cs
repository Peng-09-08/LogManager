using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManager
{
    internal class Log
    {
        /// <summary>
        /// Log型態
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// 一般資訊
            /// </summary>
            Info = 0,

            /// <summary>
            /// 警告資訊
            /// </summary>
            Warring = 1,

            /// <summary>
            /// 錯誤資訊
            /// </summary>
            Error = 2,

            /// <summary>
            /// 系統資訊
            /// </summary>
            System = 3,

            /// <summary>
            /// 例外處裡資訊
            /// </summary>
            Execption = 4,

            /// <summary>
            /// 生產流程資訊
            /// </summary>
            Produce = 5,
        }

        /// <summary>
        /// Log資訊
        /// </summary>
        public class LogInfo
        {
            /// <summary>
            /// 訊息
            /// </summary>
            public string msg;

            /// <summary>
            /// 型態
            /// </summary>
            public LogType type;
        }

        private static Queue<LogInfo> _LogQueue = new Queue<LogInfo>();

        /// <summary>
        /// 寫入Log
        /// </summary>
        /// <param name="msg">訊息</param>
        public static void WriteLine(string msg)
        {
            Enqueue(LogType.Info, msg);
        }

        /// <summary>
        /// 寫入Log
        /// </summary>
        /// <param name="type">型態</param>
        /// <param name="msg">訊息</param>
        public static void WriteLine(LogType type, string msg)
        {
            Enqueue(type, msg);
        }

        /// <summary>
        /// 寫入Log
        /// </summary>
        /// <param name="format">format</param>
        /// <param name="args">args</param>
        public static void WriteLine(string format, params object[] args)
        {
            Enqueue(LogType.Info, string.Format(format, args));
        }

        /// <summary>
        /// 寫入Log
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="format">format</param>
        /// <param name="args">args</param>
        public static void WriteLine(LogType type, string format, params object[] args)
        {
            Enqueue(type, string.Format(format, args));
        }

        /// <summary>
        /// Log佇列
        /// </summary>
        /// <param name="type">型態</param>
        /// <param name="msg">訊息</param>
        private static void Enqueue(LogType type, string msg)
        {
            lock (_LogQueue)
            {
                LogInfo info = new LogInfo();
                info.type = type;
                info.msg = string.Format("{0} {1}", DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"), msg);

                _LogQueue.Enqueue(info);
            }

#if _DEBUG
            //Console.WriteLine(msg);
#endif
        }
    }
}
