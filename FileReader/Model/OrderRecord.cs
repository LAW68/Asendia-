using FileReader.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Model
{
    public class OrderRecord
    {
        public string OrderNo;
        public string ConsignmentNo;
        public string ConsigneeName;
        public string Address1;
        public string Address2;
        public string City;
        public string State;
        public string Country;
        public string ParcelNo;
        public int ItemQuantity;
        public decimal ItemValue;
        public decimal ItemWeight;
        public string ItemDescription;
        public string ItemCurrency;
        [NonSerialized] public ReportErrorCodes reportErrorCode;
        [NonSerialized] public string errorMessage;
    }
}
