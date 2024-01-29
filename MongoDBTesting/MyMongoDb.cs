namespace ErikaBladh.MongoDBTesting;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

public class MyMongoDb : IDatabase
{
	private IMongoDatabase _database;

	public MyMongoDb(IMongoDatabase database)
	{
		_database = database;
	}

	public bool Create<T>(T item) where T : class
	{
		try
		{
			_database.GetCollection<T>(typeof(T).Name).InsertOne(item);
			return true;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e.Message);
			return false;
		}
	}

	public bool CreateMany<T>(IEnumerable<T> items) where T : class
	{
		try
		{
			if (items.Contains(null))
				throw new NullReferenceException("One or more of the provided items were null.");
			_database.GetCollection<T>(typeof(T).Name).InsertMany(items);
			return true;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e.Message);
			return false;
		}
	}

	public List<T>? Read<T>(T? filter = null) where T : class
	{
		try
		{
			if (filter is null) return _database.GetCollection<T>(typeof(T).Name).
					Find(Builders<T>.Filter.Empty).ToList();
			else return _database.GetCollection<T>(typeof(T).Name).Find(GetFilter(filter)).ToList();
		}
		catch (Exception e)
		{
			Debug.WriteLine(e.Message);
			return null;
		}
	}

	public bool Update<T>(T item) where T : class
	{
		try
		{
			if (!typeof(T).GetProperties().Select(p => p.Name)
				.Contains(typeof(MongoIdentity).GetProperties().First().Name))
				throw new MongoException(
					  $"{typeof(T).Name} doesn't contain an \"{typeof(MongoIdentity).GetProperties().First().Name}\" property.");
			var filter = GetFilter(item, true);
			var response = _database.GetCollection<T>(
				typeof(T).Name
				).ReplaceOne(filter, item, new ReplaceOptions { IsUpsert = true });
			if (!response.IsAcknowledged) return false;
			if (response.UpsertedId != null)
				typeof(T).GetProperty(
					typeof(MongoIdentity).GetProperties().First().Name
					)!.SetValue(item, response.UpsertedId.AsObjectId);
			return response.IsAcknowledged;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e.Message);
			return false;
		}
	}

	public bool Delete<T>(T item) where T : class
	{
		try
		{
			var response = _database.GetCollection<T>(typeof(T).Name).DeleteOne(GetFilter(item, true));
			if (!response.IsAcknowledged) return false;
			else return true;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e.Message);
			return false;
		}
	}

	public static IMongoDatabase GetDatabase(string connectionString, string databaseName)
	{
		var client = new MongoClient(connectionString);
		return client.GetDatabase(databaseName);
	}

	public IMongoClient GetClient()
	{
		if (_database is null)
			throw new NullReferenceException("Database is null. Intialize with GetDatabase-method first.");
		return _database.Client;
	}

	private FilterDefinition<T> GetFilter<T>(T filter, bool idOnly = false) where T : class
	{
		if (idOnly)
		{
			if (!typeof(T).GetProperties().Select(p => p.Name).Contains(
				typeof(MongoIdentity).GetProperties().First().Name)
				) throw new MongoException($"{typeof(T).Name} doesn't contain a \"{typeof(MongoIdentity).GetProperties().First().Name}\" property.");
			return Builders<T>.Filter.Eq("_id", typeof(T).GetProperty(
				typeof(MongoIdentity).GetProperties().First().Name)!.GetValue(filter));
		}
		var filterList = new List<FilterDefinition<T>>();
		foreach (var prop in typeof(T).GetProperties())
		{
			if (prop.GetValue(filter) != null)
			{
				if (prop.PropertyType == typeof(ObjectId) &&
					(ObjectId)prop.GetValue(filter)! == ObjectId.Empty) continue;
				filterList.Add(Builders<T>.Filter.Eq(prop.Name, prop.GetValue(filter)));
			}
		}
		return filterList.Count > 0 ? Builders<T>.Filter.And(filterList) : Builders<T>.Filter.Empty;
	}
}
