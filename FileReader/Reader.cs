using FileCommon;
using FileReader.Files;
using FileReader.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileReader
{
    public class Reader
    {
        public const string FILE_HEADER = "Order";
        public const string FILE_TRAILER = "EOF";

        private ArrayList _OrdersArray;

        public static string m_filename;
        public static string m_filetype;
        public static string m_operatorId;
        public static string m_inputdate;
        private int m_fieldPosition;

        public Reader()
        {
        }

        public void LoadFile(JobDetails CurrentJob, List<OrderRecord> OrdersRecordList)
        {
            CurrentJob.RecordsProcessed = 0;
            int ControlRecordsToProcess = 0;

            // Check for an input file
            if (FileIO.DoesFileExist(CurrentJob.InputFileName))
            {
                CreateCopyFileName(CurrentJob);

                CopyInputFile(ref CurrentJob);

                CurrentJob.RecordsToProcess = FileIO.CountRecordsToProcess(CurrentJob.InputFileName, CurrentJob.EOL);

                CurrentJob.JobId = 1;
                LogFiles.WriteLogFileHeader(CurrentJob);

                if (!FileIO.OpenInputFile(CurrentJob.InputFileName))
                {
                    LogFiles.WriteLogFileMessage("Error opening input file. " + CurrentJob.InputFileName);
                }

                string line = "";
                bool processedEndOfFile = false;
                while ((!processedEndOfFile) && ((line = FileIO.InputFile.ReadLine()) != null))
                {
                    if (!CheckFileHeader(line))
                    {
                        m_fieldPosition = 0;
                        if (FileIO.CheckForInputFileEOF())
                        {
                            if (!CheckFileTrailer(line))
                            {
                                AddRecordToList(line, CurrentJob, OrdersRecordList);
                                CurrentJob.RecordsProcessed++;
                            }
                            processedEndOfFile = true;
                        }
                        else
                        {
                            AddRecordToList(line, CurrentJob, OrdersRecordList);
                            CurrentJob.RecordsProcessed++;
                        }
                    }
                }
            }
            else
            {
                LogFiles.WriteLogFileMessage("Searching for Input file..." + CurrentJob.InputFileName + "...not found");
            }
        }

        protected void AddRecordToList(string line, JobDetails jobDetails, List<OrderRecord> OrdersRecordList)
        {
            OrdersRecordList.Add(GetOrdersRecord(line, jobDetails));
        }

        private OrderRecord GetOrdersRecord(string line, JobDetails jobDetails)
        {
            OrderRecord ordersRecord = new OrderRecord();
            try
            {
                _OrdersArray = GetArrayFromFileLine(line, jobDetails.ColumnSeperator);
                ordersRecord.OrderNo = _OrdersArray[0].ToString();
                ordersRecord.ConsignmentNo = _OrdersArray[1].ToString().Trim();
                ordersRecord.ParcelNo = _OrdersArray[2].ToString();
                ordersRecord.ConsigneeName = _OrdersArray[3].ToString();
                ordersRecord.Address1 = _OrdersArray[4].ToString();
                ordersRecord.Address2 = _OrdersArray[5].ToString();
                ordersRecord.City = _OrdersArray[6].ToString();
                ordersRecord.State = _OrdersArray[7].ToString().Trim();
                ordersRecord.Country = _OrdersArray[8].ToString().Trim();
                ordersRecord.ItemQuantity = int.Parse(_OrdersArray[9].ToString());
                ordersRecord.ItemValue = decimal.Parse(_OrdersArray[10].ToString());
                ordersRecord.ItemWeight = decimal.Parse(_OrdersArray[11].ToString().Trim());
                ordersRecord.ItemDescription = _OrdersArray[12].ToString().Trim();
                ordersRecord.ItemCurrency = string.IsNullOrEmpty(_OrdersArray[13].ToString().Trim()) ? "GBP" : _OrdersArray[13].ToString().Trim();
            }
            catch (Exception ex)
            {
                ordersRecord.errorMessage = ex.Message;
                ordersRecord.reportErrorCode = ReportErrorCodes.IncorrectRecordFormat;
            }
            return ordersRecord;
        }

        protected ArrayList GetArrayFromFileLine(string line, string columnSeperator)
        {
            ArrayList FileRow = new ArrayList();
            string[] strLine = null;

            if (columnSeperator == ",")
                strLine = line.Split(',');
            else if (columnSeperator == "|")
                strLine = line.Split('|');

            foreach (string key in strLine)
            {
                FileRow.Add(key);
            }
            return FileRow;
        }

        private void CopyInputFile(ref JobDetails CurrentJob)
        {
            if (CurrentJob.CopyFileName.Length > 0)
            {
                if (!FileIO.CopyInputFile(CurrentJob.InputFileName, CurrentJob.CopyFileName))
                {
                    throw new Exception("Error trying to copy input file: " + CurrentJob.InputFileName + " file to " + CurrentJob.CopyFileName + ".");
                }
                CurrentJob.InputFileName = CurrentJob.CopyFileName;
            }
        }

        internal static bool CheckFileHeader(string line)
        {
            if (line.IndexOf(FILE_HEADER) != -1)
            {
                string[] list = line.Split('|');
                return true;
            }
            return false;
        }

        internal static bool CheckFileTrailer(string line)
        {
            if (line.IndexOf(FILE_TRAILER) != -1)
            {
                string[] list = line.Split('|');
                return true;
            }
            return false;
        }

        internal static void CreateCopyFileName(JobDetails CurrentJob)
        {
            FileInfo fileInfo = new FileInfo(CurrentJob.InputFileName);
            CurrentJob.DirectoryName = fileInfo.DirectoryName.Replace("Input", "Processed");
            CurrentJob.CopyFileName = CurrentJob.DirectoryName + "\\" + fileInfo.Name;
        }

        internal static bool CopyInputFile(string InputFileName, string MoveFileName)
        {
            bool bRet = false;

            try
            {
                File.Move(InputFileName, MoveFileName);
                bRet = true;
            }
            catch (Exception e)
            {
                LogFiles.WriteLogFileMessage("Error trying to copy input file: " + InputFileName + " to " + MoveFileName + " ." + e.Message);
            }
            return bRet;
        }
    }
}