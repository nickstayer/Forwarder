using System;
using System.IO;

namespace Forwarder
{
    public class Logger
    {
        private readonly string _file;
        private Logger(string file)
        {
            _file = file;
        }

        public void Write(string _event)
        {
            string line;
            if(_event.StartsWith(Environment.NewLine))
                line = $"{Environment.NewLine}{DateTime.Now}: {_event.Substring(Environment.NewLine.Length)}";
            else line = $"{DateTime.Now}: {_event}";
            using (StreamWriter sw = new StreamWriter(_file, true))
            {
                sw.WriteLine(line);
            }
            Console.WriteLine(line);
        }

        public static Logger UpdateLogFileName(Logger logger = null)
        {
            var currentPath = Directory.GetCurrentDirectory();
            var logsDirFullName = Path.Combine(currentPath, Consts.LOGS_DIR_NAME);
            if (!Directory.Exists(logsDirFullName))
                Directory.CreateDirectory(logsDirFullName);
            var today = DateTime.Today.ToString(Consts.LOG_DATE_FORMAT_FOR_FILE_NAME);
            var logFile = Path.Combine(logsDirFullName, today + Consts.FILE_LOG_NAME);
            if (logger == null || !logger._file.Equals(logFile))
            {
                return new Logger(logFile);
            }
            return logger;
        }
    }
}