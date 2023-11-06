using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class Cart
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("UserID")]
    public string UserID { get; set; }

    [BsonElement("Items")]
    public List<CartItem> Items { get; set; }
}




