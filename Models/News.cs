using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models
{
    public class News
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        // public int ID{ get; set; }
        public string? Title { get; set; }
        public string? SContents { get; set; }

        public string? Contents { get; set; }

        public string? Thumb { get; set; }

        public bool Published { get; set; }

        public string? Alias { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreateDate { get; set; }

        public string? Author { get; set; }

        public string? Tags { get; set; }

        public int? Views { get; set; }
    }
}