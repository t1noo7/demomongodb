using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models
{
    public class Lessons
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Course { get; set; }

        public string YouTubeUrl { get; set; }

        public string YouTubeVideoId { get; set; }

        public string Thumb { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreateDate { get; set; }

        public bool Active { get; set; }
    }
}