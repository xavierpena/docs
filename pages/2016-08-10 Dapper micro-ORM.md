# Dapper micro-ORM


## Source

Pluralsight course: ".NET Micro ORMs"
by Steve Michelotti
https://app.pluralsight.com/library/courses/dotnet-micro-orms-introduction

The course also explains "why" micro-ORMS and a bit more about them. Also includes tutorials on other micro-ORMs like OrmLite, Massive, PetaPoco and Simple.Data. Dapper is the most famous and with better performance. Dapper is developed and used by the Stack Overflow team, focusing on raw performance as the primary aim.

## Setting it up

Install **Dapper** from **Nuget**.

Repository pattern:
* Create the model Contact.cs with all the columns of the table as class properties.
* Create the Interface: IContactRepository.cs
* Create ContactRepository : IContactRepository

## Test first

Create a UnitTest project "ProjName.Tests", then add new Unit Test "ContactRepositoryTests".

This is in fact an **integration test**, NOT a **unit test**:

	[TestMethod]
	public void Get_all_should_return_6_results()
	{
		// arrange
		IContactRepository repository = CreateRepository();
		
		// act
		var contacts = repository.GetAll();
		
		// assert
		// !!! This part uses "using FluentAssertions;":
		contacts.Showld().NotBeNull();
		contacts.Count.Should().Be(6)
	}

The AAA (Arrange, Act, Assert) pattern is a common way of writing unit tests for a method under test.
* The **Arrange** section of a unit test method initializes objects and sets the value of the data that is passed to the method under test.
* The **Act** section invokes the method under test with the arranged parameters.
* The **Assert** section verifies that the action of the method under test behaves as expected.

To run tests in order:
* Right click on the "ProjName.Tests"
* Add => Ordered Test
* (before they sho up, we need to Build the project)
* Sort the tests
* It generates a "BlahBlah.orderedtest" in the project


## Dapper with Repository Pattern

This is a good blog post to get the code up and running:
http://venkatbaggu.com/use-dapper-net-orm-in-asp-net-mvc/

	// poco: User.cs
	public class User
	{
	    public int UserID { get; set; }
	    public string FirstName { get; set; }
	    public string LastName { get; set; }
	    public string Email { get; set; }
	}
	
	// IRepository: IUserRepository.cs
	public interface IUserRepository
	{
	    List<User> GetAll();
	    User Find(int id);
	    User Add(User user);
	    User Update(User user);
	    void Remove(int id);
	}
	
	// Repository implementation: UserRepository.cs
	public class UserRepository : IUserRepository
	{
		private IDbConnection _db = new SqlConnection("your_connection_string_here");
		
		public List<User> GetAll() => this._db.Query<User>("SELECT * FROM Users").ToList();
	
		public User Find(int id) => this._db.Query<User>("SELECT * FROM Users WHERE UserID = @UserID", new { id }).SingleOrDefault();
		
		public User Add(User user)
		{
			var sqlQuery = "INSERT INTO Users (FirstName, LastName, Email) VALUES(@FirstName, @LastName, @Email); " + "SELECT CAST(SCOPE_IDENTITY() as int)";
			var userId = this._db.Query<int>(sqlQuery, user).Single();
			user.UserID = userId;
			return user;
		}
	
		public User Update(User user)
		{
			var sqlQuery =
				"UPDATE Users " +
				"SET FirstName = @FirstName, " +
				"    LastName  = @LastName, " +
				"    Email     = @Email " +
				"WHERE UserID = @UserID";
			this._db.Execute(sqlQuery, user);
			return user;
		}
	
		public void Remove(int id)
		{
			throw new NotImplementedException();
		}
	}

[ This youtube video](https://www.youtube.com/watch?v=G6o9ilh6uBY) demonstrates how to use the previous code.

### Query vs Execute

If you want something back from the DB, you use the Query method:

	var myResponse = db.Query<ResponseType>(...)

If you don't need to get anything back, you use the Execute method:
	db.Execute(...)

## Getting multiple results

Let's say we have Contact, and we have Address which has a ContactId. So a Contact can have multiple Addresses.

We will add "public List<Address> Addresses { get; private set; }" to the Contact poco.

	public Contact GetFullContact(int id)
	{
		var sql = 
			"SELECT * FROM Contacts WHERE ID = @Id;" +
			"SELECT * FROM Addresses WHERE ContactId = @Id";
		
		using (var multipleResults = db.QueryMultiple(sql, new { id }))
		{
			var contact = multipleResults.Read<Contact>().SingleOrDefault();
			var addresses = multipleResults.Read<Address>().ToList();
			if(contact != null && addresses != null)
			{
				contact.Addresses.AddRange(addresses);
			}
			return contact;
		}
	}

In order to insert/update multiple things, we use TransactionScope():

	// the connection must be open to use TransactionScope: db.Open()
	using(var scope = new TransactionScope())
	{
		...
		
		scope.Clomplete();
	}

## Stored procedures

So you have a stored procedure called "MyStoredProcedure":

	public Contact GetFullContact(int id)
	{
		using (var multipleResults = db.QueryMultiple("MyStoredProcedure", new { id }, CommandType: CommandType.StoredProcedure))
		{
			var contact = multipleResults.Read<Contact>().SingleOrDefault();
			var addresses = multipleResults.Read<Address>().ToList();
			if(contact != null && addresses != null)
			{
				contact.Addresses.AddRange(addresses);
			}
			return contact;
		}
	}
 
...or if we want to use a custom stored procedure with custom parameters:

	var parameters = new DynamicParameters();
	parameters.Add("@Id", value: contact.Id, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
	parameters.Add("@FirstName", contact.FirstName);
	parameters.Add("@LastName", contact.LastName);
	parameters.Add("@Company", contact.Company);
	
	db.Execute("MyStoredProcedure", parameters, commandType: CommandType.StoredProcedure);
	contact.Id = parameters.Get<int>("@Id");

DynamicParameters could also be created as:

	var parameters = new DynamicParameters(new
		{
			FirstName = contact.FirstName
			LastName = contact.LastName
			Company = contact.Company
		});
	// The Id parameter needs to be inserted appart because of its particular ParameterDirection:
	parameters.Add("@Id", value: contact.Id, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);	
	
	db.Execute("MyStoredProcedure", parameters, commandType: CommandType.StoredProcedure);
	contact.Id = parameters.Get<int>("@Id");

## Bulk Insert

	var contacts = new List<Contact>()
	{
		new Contact { FirstName = "1", LastName = "1"},
		new Contact { FirstName = "2", LastName = "2"},
		new Contact { FirstName = "3", LastName = "3"},
		new Contact { FirstName = "4", LastName = "4"}
	}
	
	var sql = 
		"INSERT INTO Contacts (FirstName, LastName, Company) VALUES (@FirstName, @LastName, @Company)" +
		"SELECT CAST(SCOPE_IDENTITY() as int)";
	
	// db must be db.Open() beforehand:
	var rowsAffected = db.Execute(sql, contacts);

## WHERE x IN (a,b,c)

Dapper automatically recoginzes an IEnumerable, and sets it as "(a,b,c)":

	var ids = new int[] {1,2,3};
	var contacts = db.Query<Contact>("SELECT * FROM Contacts WHERE ID IN @Ids", new { Ids = ids }).ToList();

## Dynamic objects

Even if using reflection one would expect slow results, Dapper is highly performant when it works with dynamic objects:

	var results = _db.Query<User>("SELECT * FROM Users").ToList();
	foreach(dynamic result in results)
	{
		var firstName = result.FirstName;
	}

## Other: Caching Repository

Cache is a hardware or software component that stores data so future requests for that data can be served faster.

The caching repository works a "wrapper" for another "basic" repository.

There is a more in-depth explanation of the Caching Repository pattern [in this blog post](http://csharpavocado.blogspot.ch/2013/03/cached-repository.html).

In our case, here is a simple example:

The "caching repository" implements the IPersonRespository interface. It has the same methods. From the outside, it looks like any other repository.

	public class CachingPersonRepository : IPersonRepository
	
		private TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);
		private DateTime _dataDateTime;
		// our real repository:
		private IPersonRespository _personRespository;
		// our temporary repository:
		private IEnumerable<Person> _cachedItems;
	
		private bool IsCacheValid()
		{
			// returns false if more than 30 seconds
		}
	
		private void ValidateCache()
		{
			// if cache is not valid:
			// * go to the "real" repository and retrieve data
			// * set _dataDateTime to Now
		}
	
		private void InvalidateCache()
		{
			// force _dataDateTime to be invalid
		}
	
		// ---
	
		public CachingPersonRepository(IPersonRespository personRespository;)
		{
			_personRepository = personRepository;
		}
	
		// ---
	
		//(other functions that implement IPersonRepository)
	
	}

Every other function that implements IPersonRespository will have IsCacheValid() first (which will update or not _cachedItems ), and then it will use _cachedItems to perform the requested action.

