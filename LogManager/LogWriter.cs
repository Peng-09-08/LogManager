using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LogManager
{
    /// <summary>
    /// 將log訊息寫入至檔案
    /// </summary>
    public static class LogWriter
    {
        private static string _logFilePath = string.Empty;
        private static bool _isInitialed = false;
        private static bool _isRun = false;
        private static StreamWriter _writer = null;

        private static object _logLock = new object();
        private static Queue<string> _logBuffer = new Queue<string>();

        /// <summary>
        /// Declare a delegate with string type argument
        /// </summary>
        /// <param name="message"></param>
        public delegate void ShowLogDelegate(string message);
        private static ShowLogDelegate _showLogHandler;

        /// <summary>
        /// Register callback function
        /// </summary>
        /// <param name="func">Function with string type argument</param>
        public static void RegisterShowLogDelegate(ShowLogDelegate func)
        {
            _showLogHandler = func;
        }

        /// <summary>
        /// Write message to log file
        /// </summary>
        /// <param name="message">Message from user</param>
        public static void WriteLog(string message)
        {
            message = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "   " + message;
            _enqueueLog(message);
            if (_showLogHandler != null)
                _showLogHandler(message);
        }

        /// <summary>
        /// Initial LogWriter
        /// </summary>
        public static void Init()
        {
            if (_isInitialed)
                return;

            try
            {
                if (string.IsNullOrEmpty(_logFilePath))
                {
                    string timeStr = DateTime.Now.ToString("yyyyMMddHHmmss");
                    _logFilePath = "Logs\\" + "Log" + timeStr + ".txt";
                }

                // get file info and dir info
                FileInfo logFileInfo = new FileInfo(_logFilePath);
                DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);

                // check dir exist
                if (!logDirInfo.Exists)
                    logDirInfo.Create();

                // delete old log when open
                if (File.Exists(_logFilePath))
                    File.Delete(_logFilePath);

                // create log
                if (!File.Exists(_logFilePath))
                {
                    using (FileStream fs = File.Create(_logFilePath))
                        fs.Close();
                }

                // create StreamWriter
                if (_writer == null)
                {
                    _writer = File.AppendText(_logFilePath);
                    _writer.AutoFlush = true;
                }

                _isInitialed = true;

                // create thread for process log
                _isRun = true;
                ThreadPool.QueueUserWorkItem(_writeLog);
            }
            catch (Exception e)
            {
                //WriteLog("Exception Message: " + e.Message + "\n" + e.StackTrace.ToString());
                MessageBox.Show(e.Message + "\n" + e.StackTrace.ToString(), "Exception Message");
            }
        }

        /// <summary>
        /// Close LogWriter
        /// </summary>
        public static void Close()
        {
            _isRun = false;
            _writer.Close();
        }

        private static void _enqueueLog(string logMessage)
        {
            lock (_logLock)
            {
                _logBuffer.Enqueue(logMessage);
            }
        }

        private static void _writeLog(object state)
        {
            while (_isRun)
            {
                while (_logBuffer.Count <= 0)
                {
                    Thread.Sleep(10);
                }

                string logMessage = string.Empty;

                lock (_logLock)
                {
                    if (_logBuffer.Count > 1000)
                        logMessage = "Fatal Error: Too many log string in queue!!";
                    else if (_logBuffer.Count > 0)
                        logMessage = _logBuffer.Dequeue();
                }

                if (!_isInitialed)
                    MessageBox.Show("Error: LogWriter didn't initialize!!");

                try
                {
                    if (_writer != null)
                        _writer.WriteLine(logMessage);
                }
                catch (Exception e)
                {
                    //WriteLog("Exception Message: " + e.Message + "\n" + e.StackTrace.ToString());
                    MessageBox.Show(e.Message + "\n" + e.StackTrace.ToString(), "Exception Message");
                    return;
                }
            }
        }
    }
}
