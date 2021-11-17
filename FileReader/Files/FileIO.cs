using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Files
{
    public class ProcessReport
    {
        public string UID = "";
        public string ErrorCode = "";

        public string ErrorMessage = "";

        public ProcessReport() { }

        public ProcessReport(string ErrorMessage)
        {
            this.ErrorMessage = ErrorMessage;
        }
    }

    public static class Report
    {
        public static IList<ProcessReport> processReportList;
    }
    public static class LogFiles
    {
        private static BinaryWriter _LogFile;
        private static BinaryWriter _RejectFile;

        public static void WriteLogFileHeader(FileCommon.JobDetails JD)
        {
            StringBuilder LogHeader = new StringBuilder();

            LogHeader.AppendFormat("Job Id:           {0}\r\n", JD.JobId);
            LogHeader.AppendFormat("Job Description:  {0}\r\n", JD.Description);
            LogHeader.AppendFormat("Job Type:         {0}\r\n", JD.Description);
            LogHeader.AppendFormat("Job Flags:        {0}\r\n", JD.JobFlags);
            LogHeader.AppendFormat("Username:         {0}\r\n", JD.Username);
            LogHeader.AppendFormat("Input Filename:   {0}\r\n", JD.InputFileName);
            LogHeader.AppendFormat("Log Filename:     {0}\r\n", JD.LogFileName);
            LogHeader.AppendFormat("Reject Filename:  {0}\r\n", JD.RejectFileName);
            LogHeader.AppendFormat("Data Supplier:    {0}\r\n", JD.DataSupplier);
            LogHeader.AppendFormat("Input Format:     {0}\r\n", JD.InputFormat);
            LogHeader.AppendFormat("Checkpoint At:    {0}\r\n", JD.CheckpointAt);
            LogHeader.AppendFormat("Transaction Size: {0}\r\n", JD.TransactionSize);
            LogHeader.AppendFormat("Max Errors:       {0}\r\n", JD.MaxErrors);
            LogHeader.Append("\r\n\r\n");

            WriteLogLine(LogHeader.ToString());
        }

        public static void WriteLogFileCheckpoint(int RecordsProcessed)
        {
            string RecordsProcessedLine = "Records Processed:        " + RecordsProcessed.ToString() + "\r\n\r\n";
            WriteLogLine(RecordsProcessedLine);
        }

        public static void WriteLogFileRecord(ProcessReport Report)
        {

            StringBuilder LogEntry = new StringBuilder();
            LogEntry.AppendFormat("{0}\r\n", DateTime.Now.ToString());
            LogEntry.AppendFormat("     UID          : {0}\r\n", Report.UID);
            LogEntry.AppendFormat("     Error Code   : {0}\r\n", Report.ErrorCode);
            LogEntry.AppendFormat("     Error Message: {0}\r\n\r\n", Report.ErrorMessage);

            WriteLogLine(LogEntry.ToString());
        }

        public static void WriteLogFileFooter(FileCommon.JobDetails JD)
        {
            StringBuilder LogFooter = new StringBuilder();

            LogFooter.AppendFormat("Records Processed:       {0}\r\n", JD.RecordsProcessed);
            LogFooter.AppendFormat("Records Inserted:        {0}\r\n", JD.RecordsInserted);
            LogFooter.AppendFormat("Records Deleted:         {0}\r\n", JD.RecordsDeleted);
            LogFooter.AppendFormat("Records Updated:         {0}\r\n", JD.RecordsUpdated);
            LogFooter.AppendFormat("Insert Records Rejected: {0}\r\n", JD.InsertRecordsRejected);
            LogFooter.AppendFormat("Delete Records Rejected: {0}\r\n", JD.DeleteRecordsRejected);
            LogFooter.AppendFormat("Update Records Rejected: {0}\r\n", JD.UpdateRecordsRejected);

            WriteLogLine(LogFooter.ToString());
        }

        public static void WriteRejectRecord(ArrayList DataLine, FileCommon.JobDetails JD)
        {
            String FileLine = "";
            if (DataLine.Count > 0)
            {
                for (int i = 0; i < DataLine.Count - 1; i++)
                {
                    FileLine += DataLine[i].ToString() + JD.ColumnSeperator;
                }
                FileLine += DataLine[DataLine.Count - 1].ToString() + JD.EOL;
            }
            WriteRejectRecord(FileLine);
        }

        public static void WriteRejectRecord(string FileLine, FileCommon.JobDetails JD)
        {
            if (_RejectFile == null)
                OpenRejectFile(JD.RejectFileName);
            WriteRejectRecord(FileLine);
        }

        private static void WriteRejectRecord(string FileLine)
        {
            FileLine += "\r\n";
            if (_RejectFile != null)
            {
                try
                {
                    _RejectFile.Write(FileLine.ToCharArray());
                    _RejectFile.Flush();
                }
                catch { }
            }
        }

        public static void WriteLogFileMessage(string Message)
        {
            Message += "\r\n";
            WriteLogLine(Message);
        }

        private static void WriteLogLine(string line)
        {
            if (_LogFile != null)
            {
                try
                {
                    _LogFile.Write(line.ToCharArray());
                    _LogFile.Flush();
                }
                catch { }
            }
        }

        public static bool OpenLogFile(string LogFileName)
        {
            bool bRet = false;
            try
            {
                _LogFile = new BinaryWriter(File.Open(LogFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite));
                bRet = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening file: " + e.Message);
            }
            return bRet;
        }

        public static bool OpenRejectFile(string RejectFileName)
        {
            bool bRet = false;
            if (_RejectFile == null)
            {
                try
                {
                    _RejectFile = new BinaryWriter(File.Open(RejectFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite));
                    bRet = true;
                }
                catch (Exception e)
                {
                    WriteLogFileMessage("Error opening file: " + e.Message);
                }
            }
            else
                bRet = true;
            return bRet;
        }
        public static void CloseAllFiles()
        {
            CloseLogFile();
            CloseRejectFile();
        }
        private static void CloseLogFile()
        {
            if (_LogFile != null) _LogFile.Close();
        }
        private static void CloseRejectFile()
        {
            if (_RejectFile != null) _RejectFile.Close();
        }

    }

    public static class FileIO
    {
        #region properties

        private static StreamReader _InputFile;
        private static StreamReader _ControlFile;
        private static StreamWriter _ReportFile;

        #endregion

        public static bool CheckForInputFileEOF()
        {
            return (_InputFile.Peek() == -1);
        }

        public static bool DoesFileExist(string Filename)
        {
            return File.Exists(Filename);
        }

        public static void WriteReportFile(FileCommon.JobDetails JD, IList<ProcessReport> processReportList)
        {
            if (JD.ReportFileName.IndexOf(".REP") != -1)
            {
                int RecordsProcess = JD.RecordsInserted + JD.RecordsDeleted;
                int RecordsRejected = JD.InsertRecordsRejected + JD.DeleteRecordsRejected;
                int totalReportLines = 0;

                using (StreamWriter writer = new StreamWriter(File.Open(JD.ReportFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
                {
                    FileInfo fileInfo = new FileInfo(JD.InputFileName);
                    string ReportHeader = JD.operatorId + "|REPORT|" + fileInfo.Name + "|" + RecordsProcess + "|" + RecordsRejected + "\r\n";
                    fileInfo = null;

                    writer.Write(ReportHeader.ToCharArray());

                    foreach (ProcessReport PR in processReportList)
                    {
                        writer.Write(PR.ErrorMessage + "\r\n");
                        totalReportLines++;
                    }
                    writer.Write(totalReportLines.ToString() + "\r\n");
                    writer.Flush();
                }
            }
        }

        public static int CountRecordsToProcess(string InputFile, string EOL)
        {
            int RecordsToProcess = 0;
            try
            {
                FileInfo fInput = new FileInfo(InputFile);
                StreamReader brInputFile = new StreamReader(fInput.OpenRead());

                while (brInputFile.Peek() != -1)
                {
                    brInputFile.ReadLine();
                    RecordsToProcess++;
                }
                brInputFile.Close();
                fInput = null;
            }
            catch (Exception e)
            {
                LogFiles.WriteLogFileMessage("Error reading input file records: " + e.Message);
                RecordsToProcess = 0;
            }
            return RecordsToProcess;
        }


        public static int ReadControlFile()
        {
            int RecordsToProcess = 0;
            try
            {
                while (_ControlFile.Peek() != -1)
                {
                    RecordsToProcess = int.Parse(_ControlFile.ReadLine());
                }
            }
            catch (Exception e)
            {
                LogFiles.WriteLogFileMessage("Error reading input file records: " + e.Message);
                RecordsToProcess = 0;
            }
            return RecordsToProcess;
        }

        public static bool OpenInputFile(string InputFileName)
        {
            bool bRet = false;
            try
            {
                FileInfo fInput = new FileInfo(InputFileName);
                _InputFile = new StreamReader(fInput.OpenRead(), Encoding.UTF8, true);
                bRet = true;
            }
            catch (Exception e)
            {
                LogFiles.WriteLogFileMessage("Error opening input file: " + e.Message);
            }
            return bRet;
        }

        public static StreamReader InputFile
        {
            get { return _InputFile; }
        }

        public static bool OpenControlFile(string ControlFileName)
        {
            bool bRet = false;
            try
            {
                FileInfo fInput = new FileInfo(ControlFileName);
                _ControlFile = new StreamReader(fInput.OpenRead());
                bRet = true;
            }
            catch (Exception e)
            {
                LogFiles.WriteLogFileMessage("Error opening input file: " + e.Message);
            }
            return bRet;
        }

        public static bool CreateDirecory(string path)
        {
            bool bRet = false;

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    bRet = true;
                }
            }
            catch (Exception e)
            {
                LogFiles.WriteLogFileMessage($"Error creating directory => {path} " + e.Message);
            }
            return bRet;
        }

        public static bool CopyInputFile(string InputFileName, string MoveFileName)
        {
            bool bRet = false;

            try
            {
                FileInfo filInfo = new FileInfo(InputFileName);
                File.Move(InputFileName, MoveFileName);
                bRet = true;
            }
            catch (Exception e)
            {
                LogFiles.WriteLogFileMessage("Error trying to copy input file: " + InputFileName + " to " + MoveFileName + " ." + e.Message);
            }
            return bRet;
        }

        public static string CreateCopyFileName(string controlInput)
        {
            FileInfo fileInfo = new FileInfo(controlInput);
            return fileInfo.DirectoryName + "\\Processed\\" + fileInfo.Name;
        }

        public static void CloseAllFiles()
        {
            CloseInputFile();
            CloseReportFile();
            CloseControlFile();
        }

        private static void CloseInputFile()
        {
            if (_InputFile != null) _InputFile.Close();
        }

        private static void CloseReportFile()
        {
            if (_ReportFile != null) _ReportFile.Close();
        }

        private static void CloseControlFile()
        {
            if (_ControlFile != null) _ControlFile.Close();
        }

        public static string FormatDate(DateTime dateTime)
        {
            string date = string.Empty;

            date = dateTime.Year.ToString();

            if (dateTime.Month.ToString().Length == 1)
                date += "0" + dateTime.Month.ToString();
            else
                date += dateTime.Month.ToString();

            if (dateTime.Day.ToString().Length == 1)
                date += "0" + dateTime.Day.ToString();
            else
                date += dateTime.Day.ToString();

            return date;
        }

        public static string FormatDateTimeAsDDMONYYYY_HH24MI(DateTime CurrentDate)
        {
            string FormattedDateTime = "";

            // Add the day
            FormattedDateTime += (CurrentDate.Day < 10) ? "0" : "";
            FormattedDateTime += CurrentDate.Day.ToString();

            // Add the month
            switch (CurrentDate.Month)
            {
                case 1:
                    FormattedDateTime += "JAN";
                    break;
                case 2:
                    FormattedDateTime += "FEB";
                    break;
                case 3:
                    FormattedDateTime += "MAR";
                    break;
                case 4:
                    FormattedDateTime += "APR";
                    break;
                case 5:
                    FormattedDateTime += "MAY";
                    break;
                case 6:
                    FormattedDateTime += "JUN";
                    break;
                case 7:
                    FormattedDateTime += "JUL";
                    break;
                case 8:
                    FormattedDateTime += "AUG";
                    break;
                case 9:
                    FormattedDateTime += "SEP";
                    break;
                case 10:
                    FormattedDateTime += "OCT";
                    break;
                case 11:
                    FormattedDateTime += "NOV";
                    break;
                case 12:
                    FormattedDateTime += "DEC";
                    break;
                default:
                    break;
            }

            // Add the year
            FormattedDateTime += CurrentDate.Year.ToString();

            FormattedDateTime += "_";

            // Add the hours
            FormattedDateTime += (CurrentDate.Hour < 10) ? "0" : "";
            FormattedDateTime += CurrentDate.Hour.ToString();

            // Add the minutes
            FormattedDateTime += (CurrentDate.Minute < 10) ? "0" : "";
            FormattedDateTime += CurrentDate.Minute.ToString();

            return FormattedDateTime;
        }

    }
}
