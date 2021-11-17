using FileReader;
using FileReader.Files;
using FileReader.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var watcher = new FileSystemWatcher(ConfigurationManager.AppSettings["ImportFolder"]);          

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.Filter = "*.csv";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            LogFiles.WriteLogFileMessage($"Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";

            var currentJob = new FileCommon.JobDetails();
            currentJob.InputFileName = e.FullPath;
            currentJob.ColumnSeperator = ",";
            currentJob.LogFileName = ConfigurationManager.AppSettings["LogFolder"] + "\\log_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".txt";

            LogFiles.OpenLogFile(currentJob.LogFileName);

            List<OrderRecord> OrdersRecordList = new List<OrderRecord>();

            var Reader = new Reader();
            Reader.LoadFile(currentJob, OrdersRecordList);

            if (OrdersRecordList.Count > 0)
            {
                var Writer = new Writer();
                Writer.WriteRecords(currentJob, OrdersRecordList);
            }

            LogFiles.WriteLogFileFooter(currentJob);
            LogFiles.CloseAllFiles();
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            LogFiles.OpenLogFile(ConfigurationManager.AppSettings["LogFolder"] + "\\log_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".txt");
            LogFiles.WriteLogFileMessage($"Deleted: {e.FullPath}");
            LogFiles.CloseAllFiles();
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            LogFiles.OpenLogFile(ConfigurationManager.AppSettings["LogFolder"] + "\\log_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".txt");
            LogFiles.WriteLogFileMessage($"Renamed:");
            LogFiles.WriteLogFileMessage($"    Old: {e.OldFullPath}");
            LogFiles.WriteLogFileMessage($"    New: {e.FullPath}");
            LogFiles.CloseAllFiles();
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                LogFiles.OpenLogFile(ConfigurationManager.AppSettings["LogFolder"] + "\\log_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".txt");
                LogFiles.WriteLogFileMessage($"Message: {ex.Message}");
                LogFiles.WriteLogFileMessage("Stacktrace:");
                LogFiles.WriteLogFileMessage(ex.StackTrace);
                LogFiles.WriteLogFileMessage(ex.InnerException.ToString());
                LogFiles.CloseAllFiles();
            }
        }
    }
}
