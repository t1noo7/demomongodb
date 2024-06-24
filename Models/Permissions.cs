using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models
{
    public class Permissions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        
        public string RoleId { get; set; }

        public List<Tasks> FunctionPermissions { get; set; } = new List<Tasks>();

    }

    public class Tasks
    {
        public string FunctionId { get; set; }

        public bool AccessPermission { get; set; }

        public bool CanCreate { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public bool CanRead { get; set; }
    }
}