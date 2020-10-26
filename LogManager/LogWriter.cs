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
        private static Queue<string> _logBuffer = new Queue<string>();
        private static string _logPath = string.Empty;
        private static bool _isInitial = false;
        private static bool _isRun = false;

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
                _logPath = Environment.CurrentDirectory + "Logs\\" + name + "_" + timeStr + ".txt";
            }

            FileInfo fileInfo = new FileInfo(_logPath);
            DirectoryInfo dirInfo = new DirectoryInfo(fileInfo.DirectoryName);

            // check dir exist
            if (!dirInfo.Exists)
                dirInfo.Create();

            // delete previous log when open
            if (File.Exists(_logPath))
                File.Delete(_logPath);

            // create log
            using (FileStream fs = File.Create(_logPath))
            {
                fs.Close();
            }

            // create StreamWriter
            if (_writer == null)
                _writer = File.AppendText(_logPath);

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
            _writer.Close();
        }

        /// <summary>
        /// 加入log訊息
        /// </summary>
        /// <param name="logMessage">訊息</param>
        public static void WriteLog(string logMessage)
        {
            lock (_lockLog)
            {
                _logBuffer.Enqueue(logMessage);
            }
        }

        private static void _writeLog(object state)
        {
            while (_isRun)
            {
                while (_logBuffer.Count <= 0)
                    Thread.Sleep(10);

                string logMessage = string.Empty;

                lock (_lockLog)
                {
                    if (_logBuffer.Count > 1000)
                        logMessage = "Fatal Error: Too many log string in queue!!";
                    else if (_logBuffer.Count > 0)
                        logMessage = _logBuffer.Dequeue();
                }

                // append log
                if (_writer != null)
                    _writer.WriteLine(logMessage);
            }
        }
    }
}
