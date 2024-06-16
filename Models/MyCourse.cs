using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models
{
    public class MyCourse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Thumb { get; set; }
        public string FullName { get; set; }
    }
}