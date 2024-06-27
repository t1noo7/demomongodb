using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models
{
    public class Orders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public double TotalAmount { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
        public string Status { get; set; }
    }

    public class OrderDetails
    {
        public string CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string Thumb { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Total => Price * Quantity;
    }
}