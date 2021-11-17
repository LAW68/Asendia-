using FileReader.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Model
{
    public class OrderList
    {
        [NonSerialized] public List<Order> Orders;
        public decimal TotalValue;
        public decimal TotalWeight;
    }
}
