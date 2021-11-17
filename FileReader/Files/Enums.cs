using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Files
{
    public enum ReportErrorCodes
    {
        Pending = 0,
        ValidProcess = 0,
        IncorrectFileFormat = 001,
        IncorrectRecordFormat = 002,
        IncorrectFieldType = 003,
        IncorrectAction = 004,
        RecordExists = 005,
        UknownError = 999
    }
}
