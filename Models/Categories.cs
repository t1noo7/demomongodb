using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models
{
    public partial class Categories
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string? CatName { get; set; }

        public string? Description { get; set; }

        public bool Published { get; set; }

        public string? Thumb { get; set; }

        public string? Title { get; set; }

        public string? Alias { get; set; }

        [BsonElement("SubCat")]
        public List<SubCat>? SubCat { get; set; }

        public string? LinkAddress { get; set; }
    }

    public class SubCat
    {
        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("link")]
        public string? Link { get; set; }

        [BsonElement("order")]
        public int Order { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}
