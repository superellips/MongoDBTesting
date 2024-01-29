namespace ErikaBladh.MongoDBTesting;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public abstract class MongoIdentity
{
	[BsonId]
	public ObjectId MongoId { get; set; }
}