using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCommon
{
    public class JobParameter
    {
        public string JobProcessingURL;
        public int BatchSize;
        public int JobTemplateID;
        public JobInputFormat inputFormat;
        public string filepath;
        public JobWorkerType jobWorkerType;
        public DateTime StartTime;

        public JobParameter()
        {
            JobTemplateID = 0;
        }
    }

    public class JobProgress
    {
        public int JobNumber;
        public int ListingsToProcess;
        public int ListingsProcessed;
        public bool StartJob;
        public string StartTime;
        public JobWorkerType jobWorkerType;

        public JobProgress()
        {
            StartJob = false;
            JobNumber = 0;
        }
    }

    public enum JobWorkerType
    {
        Auto = 0,
        Manual = 1
    }

    public enum JobInputFormat
    {
        None = 0,
        Subscriber = 1,
        Billing = 2,
        CGI = 3,
        LAC = 4,
        Locality = 5,
        LocalityGeoRef = 6,
        LocalityAlias = 7,
        EmergencyAuthority = 8,
        EmergencyAuthorityData = 9
    }

    public class JobError
    {
        public string ErrorMessage;
        public string StackTrace;
        public int JobID;
        public string InputFormat;
    }

    public class JobDetails
    {
        public int JobId = 0;
        public int JobTemplateId = 0;
        public string Description = "";
        public string JobType = "";
        public int JobFlags = 0;
        public int CheckpointAt = 1000;
        public int TransactionSize = 0;
        public int MaxErrors = 0;
        public int StartingRecord = 0;
        public int EndingRecord = 0;
        public int RecordsToProcess = 0;
        public string Username = "";
        public string StartTime = "";
        public string EndTime = "";
        public string DirectoryName = "";
        public string InputFileName = "";
        public string ControlFileName = "";
        public JobInputFormat InputFormat = JobInputFormat.None;
        public string DataSupplier = "";
        public string RejectFileName = "";
        public string LogFileName = "";
        public string ReportFileName = "";
        public string CopyFileName = "";
        public int CompletionPercentage = 0;
        public int RecordsProcessed = 0;
        public int RecordsInserted = 0;
        public int RecordsDeleted = 0;
        public int RecordsUpdated = 0;
        public int InsertRecordsRejected = 0;
        public int DeleteRecordsRejected = 0;
        public int UpdateRecordsRejected = 0;
        public string Status = "";
        public string ColumnSeperator = "";
        public string EOL = "";
        public string operatorId = "";
    }
}
