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
    public class Writer
    {
        private List<OrderList> _orders;

        public Writer()
        {
            _orders = new List<OrderList>();
            _orders.Add(new OrderList());
        }

        public void WriteRecords(JobDetails CurrentJob, List<OrderRecord> OrdersRecordList)
        {
            ProcessOrderRecords(OrdersRecordList);
            WriteXML(CurrentJob);
        }

        private void WriteXML(JobDetails CurrentJob)
        {
            XmlSerializer writer = new XmlSerializer(this._orders.GetType());
            var path = CurrentJob.DirectoryName + "\\SerializationOrders.xml";
            System.IO.FileStream file = System.IO.File.Create(path);
            writer.Serialize(file, this._orders);
            file.Close();
        }

        private void ProcessOrderRecords(List<OrderRecord> OrdersRecordList)
        {
            foreach (var OrderLine in OrdersRecordList)
            {
                AddOrder(OrderLine);
                AddConsignment(OrderLine);
                AddConsignmentParcel(OrderLine);
                AddConsignmentParcelItems(OrderLine);
                OrderLine.reportErrorCode = ReportErrorCodes.ValidProcess;
            }
        }

        private void AddOrder(OrderRecord OrderLine)
        {
            foreach (var orders in this._orders)
            {
                if (orders.Orders == null) orders.Orders = new List<Order>();

                if (!OrderNoExists(OrderLine.OrderNo))
                    orders.Orders.Add(new Order()
                    {
                        Consignments = new List<Consignment>(),
                        OrderNo = OrderLine.OrderNo
                    });
            }
        }

        private void AddConsignment(OrderRecord OrderLine)
        {
            foreach (var orders in this._orders)
            {
                foreach (var order in orders.Orders)
                {
                    if (!ConsignmentNoExists(OrderLine.ConsignmentNo)
                        && order.OrderNo == OrderLine.OrderNo)
                    {
                        order.Consignments.Add(new Consignment()
                        {
                            Address1 = OrderLine.Address1,
                            Address2 = OrderLine.Address2,
                            City = OrderLine.City,
                            ConsigneeName = OrderLine.ConsigneeName,
                            ConsignmentNo = OrderLine.ConsignmentNo,
                            Country = OrderLine.Country,
                            parcels = new List<Parcel>(),
                            State = OrderLine.State
                        });
                    }
                }
            }
        }

        private void AddConsignmentParcel(OrderRecord OrderLine)
        {
            foreach (var orders in this._orders)
            {
                foreach (var order in orders.Orders)
                {
                    foreach (var consignment in order.Consignments)
                    {
                        if (!ParcelNoExists(OrderLine.ParcelNo)
                            && OrderLine.OrderNo == order.OrderNo
                            && consignment.ConsignmentNo == OrderLine.ConsignmentNo)
                        {
                            consignment.parcels.Add(new Parcel()
                            {
                                ParcelItems = new List<ParcelItem>(),
                                ParcelNo = OrderLine.ParcelNo
                            });
                        }
                    }
                }
            }
        }

        private void AddConsignmentParcelItems(OrderRecord OrderLine)
        {             
            foreach (var orders in this._orders)
            {
                foreach (var order in orders.Orders)
                {
                    foreach (var consignment in order.Consignments)
                    {
                        foreach (var parcel in consignment.parcels)
                        {
                            if (parcel.ParcelNo == OrderLine.ParcelNo
                                && order.OrderNo == OrderLine.OrderNo
                                && consignment.ConsignmentNo == OrderLine.ConsignmentNo)
                            {
                                parcel.ParcelItems.Add(new ParcelItem()
                                {
                                    ItemQuantity = OrderLine.ItemQuantity,
                                    ItemValue = OrderLine.ItemValue,
                                    ItemWeight = OrderLine.ItemWeight,
                                    ItemDescription = OrderLine.ItemDescription,
                                    IemCurrency = OrderLine.ItemCurrency
                                });
                                orders.TotalValue += OrderLine.ItemValue;
                                orders.TotalWeight += OrderLine.ItemWeight;
                            }
                        }
                    }
                }
            }
        }

        private bool OrderNoExists(string orderNo)
        {
            var exist = this._orders.Any(o => o.Orders.Any(s => s.OrderNo == orderNo));
            return exist;           
        }

        private bool ConsignmentNoExists(string consignmentNo)
        {
            var exist = this._orders.Any(o => o.Orders
            .Any(s => s.Consignments
            .Any(c => c.ConsignmentNo == consignmentNo)));
            return exist;
        }

        private bool ParcelNoExists(string parcelNo)
        {
            var exist = this._orders.Any(o => o.Orders
           .Any(s => s.Consignments
           .Any(t => t.parcels
           .Any(p => p.ParcelNo == parcelNo))));
            return exist;           
        }
    }
}