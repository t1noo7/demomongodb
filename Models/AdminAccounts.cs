using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models;

public partial class AdminAccounts
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }

    public string? FullName { get; set; }

    public DateTime? Birthday { get; set; }

    public string? Avatar { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? Password { get; set; }

    public string? Salt { get; set; }

    public DateTime? LastLogin { get; set; }
    public string RoleId { get; set; }

    public string Role { get; set; }

    public bool Active { get; set; }
}
