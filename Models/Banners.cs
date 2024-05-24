using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoMongoDB.Models;

public partial class Banners
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }

    public string? BannerName { get; set; }

    public string? Thumb { get; set; }

    public DateTime? DateModified { get; set; }

    public bool Active { get; set; }

    public string? BannerText { get; set; }

    public bool ActiveButton { get; set; }

    public string? ButtonText { get; set; }

    public string? BannerHeaderText { get; set; }

    public int OrderIndex { get; set; }
}
