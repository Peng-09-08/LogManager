using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace LogManager
{
    /// <summary>
    /// 將log訊息寫入至檔案
    /// </summary>
    public class LogWriter
    {
        private static StreamWriter _writer = null;
        private static object _lockLog = new object();
        private static Queue<LogInfo> _logBuffer = new Queue<LogInfo>();
        private static string _logPath = string.Empty;
        private static bool _isInitial = false;
        private static bool _isRun = false;
        private static Dictionary<LogType, string> _pairMessage = new Dictionary<LogType, string>()
        {
            { LogType.Info, "[Information]" },
            { LogType.Warning, "[===Warning===]" },
            { LogType.Error, "[####Error####]" },
            { LogType.Exception, "[!!Exception!!]" },
            { LogType.System, "[**System**]" },
        };

        /// <summary>
        /// Log型態
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// 無指定型態
            /// </summary>
            None = -1,

            /// <summary>
            /// 一般資訊
            /// </summary>
            Info,

            /// <summary>
            /// 警告資訊
            /// </summary>
            Warning,

            /// <summary>
            /// 錯誤資訊
            /// </summary>
            Error,

            /// <summary>
            /// 例外處裡資訊
            /// </summary>
            Exception,

            /// <summary>
            /// 系統資訊
            /// </summary>
            System,

            ///// <summary>
            ///// 生產流程資訊
            ///// </summary>
            //Produce,
        }

        /// <summary>
        /// Log資訊
        /// </summary>
        public class LogInfo
        {
            /// <summary>
            /// 訊息
            /// </summary>
            public string Message;

            /// <summary>
            /// Log型態
            /// </summary>
            public LogType Type;

            /// <summary>
            /// 建構式
            /// </summary>
            /// <param name="msg">訊息</param>
            /// <param name="type">Log型態</param>
            public LogInfo(string msg, LogType type)
            {
                Message = msg;
                Type = type;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">log檔名稱</param>
        public static void Init(string name)
        {
            if (_isInitial)
                return;

            if (_logPath == string.Empty)
            {
                string timeStr = DateTime.Now.ToString("yyyyMMddHHmmss");
                _logPath = Environment.CurrentDirectory + "\\Logs\\" + name + "_" + timeStr + ".txt";
            }

            FileInfo fileInfo = new FileInfo(_logPath);
            DirectoryInfo dirInfo = new DirectoryInfo(fileInfo.DirectoryName);

            // check dir exist
            if (!dirInfo.Exists)
                dirInfo.Create();

            // create StreamWriter
            if (_writer == null)
            {
                _writer = new StreamWriter(_logPath, false);
                _writer.AutoFlush = true;
            }

            _isInitial = true;

            // create thread for process log
            _isRun = true;
            ThreadPool.QueueUserWorkItem(_writeLog);
        }

        /// <summary>
        /// 關閉LogWriter
        /// </summary>
        public static void Close()
        {
            _isRun = false;
            _logBuffer.Clear();
            if (_writer != null)
                _writer.Close();
        }

        /// <summary>
        /// 寫入一般log訊息
        /// </summary>
        /// <param name="message">訊息</param>
        public static void WriteLog(string message)
        {
            lock (_lockLog)
            {
                _logBuffer.Enqueue(new LogInfo(message, LogType.None));
            }
        }

        /// <summary>
        /// 寫入information訊息
        /// </summary>
        /// <param name="message">訊息</param>
        public static void WriteInfoLog(string message)
        {
            lock (_lockLog)
            {
                _logBuffer.Enqueue(new LogInfo(message, LogType.Info));
            }
        }

        /// <summary>
        /// 寫入警告訊息
        /// </summary>
        /// <param name="message">訊息</param>
        public static void WriteWarningLog(string message)
        {
            lock (_lockLog)
            {
                _logBuffer.Enqueue(new LogInfo(message, LogType.Warning));
            }
        }

        /// <summary>
        /// 寫入錯誤訊息
        /// </summary>
        /// <param name="message">訊息</param>
        public static void WriteErrorLog(string message)
        {
            lock (_lockLog)
            {
                _logBuffer.Enqueue(new LogInfo(message, LogType.Error));
            }
        }

        /// <summary>
        /// 寫入例外訊息
        /// </summary>
        /// <param name="message">訊息</param>
        public static void WriteExceptionLog(string message)
        {
            lock (_lockLog)
            {
                _logBuffer.Enqueue(new LogInfo(message, LogType.Exception));
            }
        }

        private static void _writeLog(object state)
        {
            while (_isRun)
            {
                while (_logBuffer.Count <= 0)
                    Thread.Sleep(10);

                lock (_lockLog)
                {
                    LogInfo info = null;
                    if (_logBuffer.Count > 1000)
                    {
                        info.Message = "Fatal Error: Too many log string in queue!!";
                        info.Type = LogType.System;
                    }
                    else if (_logBuffer.Count > 0)
                        info = _logBuffer.Dequeue();

                    // append log
                    if (_writer != null)
                    {
                        string str = string.Format("{0}   {1}   {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            _pairMessage[info.Type], info.Message);

                        _writer.WriteLine(str);
                    }
                }
            }
        }
    }
}
