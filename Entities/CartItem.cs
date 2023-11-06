using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class CartItem
{
    [BsonElement("CourseId")]
    public string CourseID { get; set; }
    public DateTime CreatedDate { get; set; } 
}
