namespace ErikaBladh.MongoDBTesting;

public interface IDatabase
{
	public bool Create<T>(T item) where T : class;
	public bool CreateMany<T>(IEnumerable<T> items) where T : class;
	public List<T>? Read<T>(T? filter = null) where T : class;
	public bool Update<T>(T item) where T : class;
	public bool Delete<T>(T filter) where T : class;
}
