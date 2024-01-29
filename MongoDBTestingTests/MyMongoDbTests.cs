namespace ErikaBladh.MongoDBTesting.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass()]
public class MyMongoDbTests
{
	private const string _testDbCon = "mongodb://localhost:27017/";
	private const string _testDbName = "MongoDbTestDatabase";
	private IDatabase? _subject;

	[TestInitialize]
	public void Setup()
	{
		_subject = new MyMongoDb(MyMongoDb.GetDatabase(_testDbCon, _testDbName));
		if (_subject is MyMongoDb) (_subject as MyMongoDb)!.GetClient().DropDatabase(_testDbName);
	}

	[TestCleanup]
	public void TearDown()
	{
		if (_subject is MyMongoDb) (_subject as MyMongoDb)!.GetClient().DropDatabase(_testDbName);
	}

	[TestMethod()]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(3)]
	[DataRow(4)]
	[DataRow(5)]
	[DataRow(6)]
	public void CreateTest_CreatePersonTest(int index)
	{
		var testObject = GetTestPerson(index);

		Assert.IsTrue(_subject!.Create(testObject!));
		Assert.IsFalse(_subject!.Create(testObject!));
	}

	[TestMethod()]
	[DataRow(7)]
	public void CreateTest_CreatingNullPersonReturnsFalse(int index)
	{
		var testObject = GetTestPerson(index);

		Assert.IsFalse(_subject!.Create(testObject!));
	}

	[TestMethod()]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(3)]
	[DataRow(4)]
	public void CreateTest_CreateOrgTest(int index)
	{
		var testObject = GetTestOrg(index);

		Assert.IsTrue(_subject!.Create(testObject!));
		Assert.IsFalse(_subject!.Create(testObject!));
	}

	[TestMethod()]
	[DataRow(5)]
	public void CreateTest_CreateNullOrgReturnsFalse(int index)
	{
		var testObject = GetTestOrg(index);

		Assert.IsFalse(_subject!.Create(testObject!));
	}

	[TestMethod()]
	public void CreateManyTest_AddRangeOfPeople()
	{
		var testObjects = _testPeople[0..7];

		Assert.IsTrue(_subject!.CreateMany(testObjects));
		Assert.IsFalse(_subject!.CreateMany(testObjects));
	}

	[TestMethod()]
	public void CreateManyTest_AddRangeOfOrgs()
	{
		var testObjects = _testOrgs[0..4];

		Assert.IsTrue(_subject!.CreateMany(testObjects));
		Assert.IsFalse(_subject!.CreateMany(testObjects));
	}

	[TestMethod()]
	[DataRow(0)]
	[DataRow(4)]
	[DataRow(5)]
	[DataRow(6)]
	public void ReadTest_ReadPerson(int index)
	{
		var testObject = GetTestPerson(index);
		_subject!.Create(testObject!);

		var actual = _subject.Read<PersonTest>();

		Assert.IsNotNull(actual);
		Assert.IsNotNull(actual[0]);
		Assert.IsInstanceOfType(actual, typeof(List<PersonTest>));
		Assert.IsInstanceOfType(actual![0], typeof(PersonTest));
		Assert.IsNotNull(actual[0].MongoId);
		Assert.AreEqual(testObject!.Name, actual[0].Name);
		Assert.AreEqual(testObject!.Age, actual[0].Age);
		Assert.AreEqual(testObject!.MongoId, actual[0].MongoId);
	}

	[TestMethod()]
	public void ReadTest_FilteringForSpecificPersonById()
	{
		_subject!.CreateMany(_testPeople[1..6]);
		_subject!.CreateMany(_testOrgs[1..4]);

		var testObject = GetTestPerson(0);
		_subject!.Create(testObject!);

		var actual = _subject!.Read(new PersonTest { MongoId = testObject!.MongoId });

		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.Count == 1);
		Assert.IsNotNull(actual[0]);
		Assert.IsInstanceOfType(actual, typeof(List<PersonTest>));
		Assert.IsInstanceOfType(actual![0], typeof(PersonTest));
		Assert.IsNotNull(actual[0].MongoId);
		Assert.AreEqual(testObject!.Name, actual[0].Name);
		Assert.AreEqual(testObject!.Age, actual[0].Age);
		Assert.AreEqual(testObject!.MongoId, actual[0].MongoId);
	}

	[TestMethod()]
	public void ReadTest_FilteringForSpecificOrgByName()
	{
		_subject!.CreateMany(_testPeople[1..6]);
		_subject!.CreateMany(_testOrgs[1..4]);

		var testObject = GetTestOrg(0);
		_subject!.Create(testObject!);

		var actual = _subject!.Read(new OrgTest { Name = testObject!.Name });

		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.Count == 1);
		Assert.IsNotNull(actual[0]);
		Assert.IsInstanceOfType(actual, typeof(List<OrgTest>));
		Assert.IsInstanceOfType(actual![0], typeof(OrgTest));
		Assert.IsNotNull(actual[0].MongoId);
		Assert.AreEqual(testObject!.Name, actual[0].Name);
		Assert.AreEqual(testObject!.Founded!.Value.Date, actual[0].Founded!.Value.Date);
		Assert.AreEqual(testObject!.MongoId, actual[0].MongoId);
		Assert.AreEqual(testObject.People!.Count, actual[0].People!.Count);
	}

	[TestMethod()]
	public void UpdateTest_UpdateOrgName()
	{
		_subject!.CreateMany(_testPeople[1..6]);
		_subject!.CreateMany(_testOrgs[1..4]);

		var testObject = GetTestOrg(0);
		_subject!.Create(testObject!);

		testObject!.Name = "Actual Org";

		Assert.IsTrue(_subject.Update(testObject!));

		var actual = _subject!.Read(new OrgTest { Name = testObject!.Name });

		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.Count == 1);
		Assert.IsNotNull(actual[0]);
		Assert.IsInstanceOfType(actual, typeof(List<OrgTest>));
		Assert.IsInstanceOfType(actual![0], typeof(OrgTest));
		Assert.IsNotNull(actual[0].MongoId);
		Assert.AreEqual(testObject!.Name, actual[0].Name);
		Assert.AreEqual(testObject!.Founded!.Value.Date, actual[0].Founded!.Value.Date);
		Assert.AreEqual(testObject!.MongoId, actual[0].MongoId);
		Assert.AreEqual(testObject.People!.Count, actual[0].People!.Count);
	}

	[TestMethod]
	public void UpdateTest_ProperlyUpsertsIfPersonDoesntExist()
	{
		_subject!.CreateMany(_testPeople[1..6]);
		_subject!.CreateMany(_testOrgs[1..4]);

		var testObject = GetTestPerson(0);

		Assert.IsTrue(_subject!.Update(testObject!));

		var actual = _subject!.Read(new PersonTest { Name = testObject!.Name });

		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.Count == 1);
		Assert.IsNotNull(actual[0]);
		Assert.IsInstanceOfType(actual, typeof(List<PersonTest>));
		Assert.IsInstanceOfType(actual![0], typeof(PersonTest));
		Assert.IsNotNull(actual[0].MongoId);
		Assert.AreEqual(testObject!.Name, actual[0].Name);
		Assert.AreEqual(testObject!.Age, actual[0].Age);
		Assert.AreEqual(testObject!.MongoId, actual[0].MongoId);
	}

	[TestMethod()]
	public void DeleteTest_DeleteOrg()
	{
		_subject!.CreateMany(_testPeople[1..6]);
		_subject!.CreateMany(_testOrgs[1..4]);

		var testObject = GetTestOrg(0);
		_subject!.Create(testObject!);

		Assert.IsTrue(_subject.Delete(testObject!));

		var actual = _subject!.Read(new OrgTest { Name = testObject!.Name });

		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.Count == 0);
		Assert.IsInstanceOfType(actual, typeof(List<OrgTest>));
	}

	private PersonTest? GetTestPerson(int index)
	{
		return _testPeople[index];
	}

	private OrgTest? GetTestOrg(int index)
	{
		return _testOrgs[index];
	}

	private class PersonTest : MongoIdentity
	{
		public string? Name { get; set; }
		public int? Age { get; set; }
	}

	private class OrgTest : MongoIdentity
	{
		public string? Name { get; set; }
		public DateTime? Founded { get; set; }
		public List<PersonTest?>? People { get; set; }
	}

	private static readonly List<PersonTest> _testPeople = new List<PersonTest>()
		{
			new PersonTest { Name = "Alice", Age = 42 },
			new PersonTest { Name = "Bob", Age = 39 },
			new PersonTest { Name = "Charlie", Age = 27 },
			new PersonTest { Name = "Daniel", Age = int.MaxValue },
			new PersonTest { Name = "Eve", Age = int.MinValue },
			new PersonTest { Name = null, Age = 19 },
			new PersonTest(),
			null
		};

	private static readonly List<OrgTest> _testOrgs = new List<OrgTest>()
		{
			new OrgTest { Name = "Argon Org", Founded = DateTime.UtcNow, People = _testPeople },
			new OrgTest { Name = "Berylium Org", Founded = DateTime.MinValue, People = _testPeople },
			new OrgTest { Name = "Cesium Org", Founded = DateTime.MaxValue, People = _testPeople },
			new OrgTest { Name = null, Founded = DateTime.UtcNow, People = null },
			new OrgTest(),
			null
		};
}