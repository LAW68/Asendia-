using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Model
{
    public class Consignment
    {
        public string ConsignmentNo;
        public string ConsigneeName;
        public string Address1;
        public string Address2;
        public string City;
        public string State;
        public string Country;
        public List<Parcel> parcels;       
    }
}
