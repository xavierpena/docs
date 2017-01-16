# Building your first API with ASP.NET Core


## Info

Pluralsight course:

"Building Your First API with ASP.NET Core"

by Kevin Dockx


## Tooling

* Visual Studio 2015, Update3 or Community Edition.
* Postman (for testing API requests): https//www.getpostman.com/
* A browser (Chrome)


## Installation

Go to:

https://www.microsoft.com/net/core#windows

Install .NET Core 1.0.1 - VS 2015 Tooling Preview 2

Visual studio -> New Project -> Templates -> Visual C# -> .NET Core

Choose "ASP.NET Core Web Application (.NET Core)"

ASP.NET Core Template: "Empty" (for this course)


## Project details

### Program.cs

In `Program.cs`, the `.UseIISIntegration()` says that VisualStudio's IIS Express works as a reverse proxy for Kestrel (the default universal server).

.UseContentRoot(...) => by default its the current directory. It's different than the "web root" (which is `base_root\wwwroot`)

### Startup.cs

`Startup.cs`: the Startup class is the entry point for an application.

For dependency injection, we use the method `ConfigureServices`.

After the `ConfigureServices` method, the application calls the `Configure` method, because it uses services that are registerd and configured in `ConfigureServices`. This method is used to specify how an ASP.NET Core application will respond to individual http requests.

### Pipeline & Middleware

Everything between the Request and the Response is the "request pipeline". We configure this "request pipeline" by adding "middleware" (examples: diagnostics, authentication...).


## Middleware example

Adding diagnostics to show the details of the exception when an exception is thrown (only shown in a development environment!).

In `Startup.cs`, method `Configure`:

	if(env.IsDevelopment())
	{
		app.UseDevelopperExceptionPage();
	}
	else
	{
		// added code:
		// logs the exception, using the ILoggerFactory injected in the method
		app.UseExceptionHandler(); 
	}

Where is "Development" set up? Go to:

project properties -> Debug -> Environment Variables -> ASPNETCORE_ENVIRONMENT: Development.

There are 3 environments: Development / Staging / Production

This info is stored in the `IHostingEnvironment` (`env`, in this function).

Changes in the configuration will not be available before restarting the server (IIS Express in this case). In VisualStudio, you need to restart VisualStudio.


## MVC pattern

To enable MVC in our project, we must go to `Startup.cs` and set:

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMvc();
	}

An error will show up, but intellisense will propose the correct NuGet package that needs to be added.

Then add MVC to the request pipeline: `app.UseMvc();` (right after the exception handler middleware that we mentioned earlier).


## Our first controller

Create a new folder: \Controllers (next to \wwwroot). Add a new file: `CitiesController.cs`:

	namespace ProjectName.API.Controllers
	{
		// The route lets us avoid putting "api/citites" at every [HttpGet("api/citites")] etc.
		// It can also be `Route("api/[controller]")`, but it has its disadvantages...
		[Route("api/cities")]
		// Make it derive from the Controller
		public class CitiesController : Controller
		{
			[HttpGet()]
			public JsonResult GetCities()
			{
				return new JsonResult(new List<object>()
				{
					new { id=1, Name="New York City" },
					new { id=2, Name="Antwerp" }
				});
			}
		}
	}


## Routing

Convention-based routing is not advised for API's, since conventions need to be configured.

We will use attribute-based routing instead.

[[div]]
||~ HTTP Method ||~ Description ||~ Attribute ||~ Level ||~ Sample URI ||
|| GET || to read an existing city/cities || HttpGet || Action || /api/cities (for a list) /api/cities/1 (for a single element) ||
|| POST || to create a new city || HttpPost || Action || /api/citites ||
|| PUT || to update all fields of an existing city || HttpPut || Action || /api/citites/1 ||
|| PATCH || to update certain fields of an existing city || HttpPatch || Action || /api/cities/1 ||
|| DELETE || to delete an existing city || HttpDelete || Action || /api/citites/1 ||
|| --- || provides a template to prefix all templates of action-level attributes || Route || Controller || ---
[[/div]]


## Testing with Postman

GET -> http://localhost:<port-number>/api/citites -> Send


## Adding a model

Add a new folder: \Models (next to \Controllers). Add `CityDto.cs`. "DTO" stands for Data Transfer Object.

	public class CityDto
	{
		public int Id {get;set;}
		public string Name {get;set;}
		public string Description {get;set;}
	}

Now add `CitiesDataStore.cs` to the root:

	public class CitiesDataStore
	{
		public static CitiesDataStore Current { get; } = new CitiesDataStore();
		public List<CityDto> Cities {get;set;}
		public CitiesDataStore()
		{
			// init dummy data:
			Cities = new List<CityDto>()
			{
				new CityDto() { id=1, Name="New York City", Description="xxx" },
				new CityDto() { id=2, Name="Antwerp", Description="yyy" }
			}
		}
	}

And now the controller should look like this:

	[HttpGet()]
	public JsonResult GetCities()
	{
		return new JsonResult(CitiesDataStore.Current.Cities);
	}

We'll use the ne datastore and add a new action:

	namespace ProjectName.API.Controllers
	{
		[Route("api/cities")]
		public class CitiesController : Controller
		{
			[HttpGet()]
			public JsonResult GetCities()
			{
				return new JsonResult(CitiesDataStore.Current.Cities);
			}
			
			[HttpGet("{id}")]
			public JsonResult GetCities(int id)
			{
				return new JsonResult(CitiesDataStore.Current.Cities
					.Where(x => x.Id == id)
					.FirstOrDefault());
			}
		}
	}

## Status codes

* Level 200: Success
 * 200: OK
 * 201: Created
 * 204: No Content
* Level 400: Client Error
 * 400: Bad Request
 * 401: Unauthorized
 * 403: Forbidden
 * 404: Not Found
 * 409: Conflict
* Level 500: Server Error
 * 500: Internal Server Error
 
To add status codes to our code:

	namespace ProjectName.API.Controllers
	{
		[Route("api/cities")]
		public class CitiesController : Controller
		{
			[HttpGet()]
			// This time we are using `IActionResult` instead of `JsonResult`. The user will be able to choose between json/xml/etc, plus it also contains the info of the status. 
			public IActionResult GetCities()
			{
				return Ok(CitiesDataStore.Current.Cities);
			}
			
			[HttpGet("{id}")]
			public IActionResult GetCities(int id)
			{
				var cityToReturn = CitiesDataStore.Current.Cities
					.Where(x => x.Id == id)
					.FirstOrDefault();
					
				if(cityToReturn == null)
					return NotFound();
				else
					return Ok(cityToReturn);
			}
		}
	}

If we want to show the status info on the browser, in `Startup.cs` we should add (just before `app.UseMvc();`): `app.UseStatusCodePages();`.


## Child resources

Let's suppose that CityDto has a property that is another object: PointsOfInterest. Its controller would be:

	[Route("api/cities")]
	public class PointsOfInterestController : Controller
	{
		[HttpGet("{cityId}/pointsofinterest")]
		public IActionResult GetPointsOfInterest(int cityId)
		{
			var city = ...
			if(city == null)
				return NotFound();
			else
				return Ok(city.PointsOfInterest);
		}
		
		[HttpGet("{cityId}/pointsofinterest/{id}")]
		public IActionResult GetPointsOfInterest(int cityId, int id)
		{
			var city = ...
			if(city == null)
				return NotFound();
				
			var pointOfInterest = ...	
			if(pointOfInterest == null)
				return NotFound();
				
			return Ok(pointOfInterest);
		}
	}


## Serializer settings

The json serializer returns the properties as cammel case (first letter as lower case), instead of just as is (which was what older ASP APIs did).

In `Startup.cs`, we can modify those parameters:

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMvc()
			.AddJsonOptions(o => {
				if(o.SerializerSettings.ContractResolver != null)
				{
					var castedResolver = o.SerializerSettings.ContractResolver
						as DefaultContractResolver;
					castedResolver.NamingStrategy = null;
				}
			});
	}

## Json? Xml?

That is: "Formatters and Content Negotiation".

Formatters:
* Output formatters: from the accept header.
* Input formatters: from the content-type header.

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMvc()
			.AddMvcOptions(o => o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()));
		// same thing for "input formatter"
	}


## Manipulating resources

Use a separate DTO for creating, for updating, and for returning resources. The reason is that you might have diferent fields (for example: you don't need an Id for creation), and you might have diferent validation rules.

### Creation

In this case we'll use `PointOfInterestForCreationDto`:

	[HttpPost("{cityId}/pointsofinterest")]
	public IActionResult CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
	{
		if(pointOfInterest == null)
			return BadRequest();
		
		...
		
		// Returns a response with a location header, which contains the URI where the newly created point of interest can be found.
		return CreatedAtRoute("GetPointsOfInterest", new { cityId = cityId, id = finalPointOfInterest.Id}, finalPointOfInterest);
	}
	
	// To make `return CreatedAtRoute();` work, add ` Name="GetPointsOfInterest"` to the Get:
	[HttpGet("{cityId}/pointsofinterest/{id}", Name="GetPointsOfInterest")]
	public IActionResult GetPointOfInterest(int cityId, int id)
	{
		...
	}


### Validation

	class PointOfInterestForCreationDto
	{
		// `ErrorMessage` is optional:
		[Required(ErrorMessage="Yous should provide a name value")]
		[MaxLength(50)]
		public string Name {get;set;}
		
		[MaxLength(200)]
		public string Description {get;set;}
	}
	
	
	[HttpPost("{cityId}/pointsofinterest")]
	public IActionResult CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
	{
		if(pointOfInterest == null)
			return BadRequest();
			
		// (*) Add custom model error (which not be feasible to add via model class):
		if(pointOfInterest.Description == pointOfInterest.Name)
			ModelState.AddModelError("Description", "The provided description should be different from the name");
			
		// Added for validation:
		if(ModelState.IsValid())
			return BadRequest();
		
		...
		
		// Returns a response with a location header, which contains the URI where the newly created point of interest can be found.
		return CreatedAtRoute("GetPointsOfInterest", new { cityId = cityId, id = finalPointOfInterest.Id}, finalPointOfInterest);
	}

(*) This is not ideal because validation happens in two separate places (the model and the controller). To fix this, you can use libraries such as FluentValidation.


### Update

	[HttpPut("{cityId}/pointsofinterest/{id}")]
	public IActionResult UpdatePointOfInterest(int cityId, int id,
		[FromBody] PointsOfInterestForUpdateDto pointOfInterest)
	{
		var city = ...
		if(city == null)
			return NotFound();
			
		var pointOfInterestFromStore = ...	
		if(pointOfInterestFromStore == null)
			return NotFound();
		
		pointOfInterestFromStore.Name = pointOfInterest.Name;
		pointOfInterestFromStore.Description = pointOfInterest.Description;
		
		return NoContent();
	}


### Partilly updating a resource

There is a standard for that:

Json Patch (RFC 6902)
https://tools.ietf.org/html/rfc6902

Describes a document structure for expressing a sequence of operations to apply to a JSON document. It is mapped in .NET core by `JsonPatchDocument`:

	[HttpPut("{cityId}/pointsofinterest/{id}")]
	public IActionResult UpdatePointOfInterest(int cityId, int id,
		[FromBody] JsonPatchDocument<PointsOfInterestForUpdateDto> patchDoc)
	{
		if(patchDoc == null)
			return NotFound();
			
		var city = ...
		if(city == null)
			return NotFound();
			
		var pointOfInterestFromStore = ...	
		if(pointOfInterestFromStore == null)
			return NotFound();
		
		var pointOfInterestToPatch = 
			new PointOfInterestForUpdateDto()
			{
				Name = pointOfInterestFromStore.Name,
				Description = pointOfInterestFromStore.Description
			};
		
		// Also check for validation errors:
		patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);
		
		if(!ModelState.IsValid)
			return BadRequest();
			
		// Add custom model error (which not be feasible to add via model class):
		if(pointOfInterest.Description == pointOfInterest.Name)
			ModelState.AddModelError("Description", "The provided description should be different from the name");
			
		TryValidateModel(pointOfInterestToPatch);
		
		if(!ModelState.IsValid)
			return BadRequest();	
			
		pointOfInterestFromStore.Name = pointOfInterest.Name;
		pointOfInterestFromStore.Description = pointOfInterest.Description;
		
		return NoContent();
	}


### Delete

	[HttpDelete("{cityId}/pointsofinterest/{id}")]
	public IActionResult UpdatePointOfInterest(int cityId, int id)
	{
		var city = ...
		if(city == null)
			return NotFound();
			
		var pointOfInterestFromStore = ...	
		if(pointOfInterestFromStore == null)
			return NotFound();
		
		// remove ...
		
		return NoContent();
	}


## Dependency injection

### Logger

The logger is a built-in service, we don't need to configure.

In the `Configure` method, `ILoggerFactory` is already injected.

	public void Configure(...)
	{
		loggerFactory.AddConsole();
		loggerFactory.AddDebug(LogLevel.Critical); // the minimum debug level is optional
		
		...
	}

Now, in the controller:

	public class PointsOfInterestController : Controller
	{
		public ILogger<PointsOfInterestController> _logger;
		
		public PointsOfInterestController(ILogger<PointsOfInterestController> logger)
		{
			_logger = logger;
			//// Alternatively, if somehow you can't use dependency injection:
			// HttpContext.RequestServices.GetService(*); * = Type ServiceType
		}
	}

Now you can use the logger inside the controller: `_logger.LogInformation("blah blah blah")`. This adds the information to the console.

If you DON'T want to log to the console, but you want to log to a file (for example).

First go to https://github.com/aspnet/Logging . At the bottom of the description you'll find the section Providers where they show a list of community projects that are available as ILogger's. For this example, we'll use NLog.

Download NLog through NuGet. In the `nlog.config`, paste this minimal configuration:

	<?xml version="1.0" encoding="utf-8">
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
		xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<targets>
			<target name="logfile" xsi:type="File" fileName="nlog-$(shortdate).log" />
		</targets>
		<rules>
			<logger name="*" minlevel="Info" writeTo="logfile" />
		</rulles>
	</nlog>

In the `Configure` method:

	public void Configure(...)
	{
		loggerFactory.AddConsole();
		loggerFactory.AddDebug(); // default: info level
		
		//// Unnecessarily long way:
		//loggerFactory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());
		
		// More direct way:
		loggerFactory.AddNLog();
		
		...
	}

In the root of the project, a new file called `nlog-yyyy-MM-dd.log` is created.


### Creating custom services

In this example we'll use a mail sender service.

First create the folder \Services in the root of the project, and add `LocalMailService.cs`. Put the necessary info in it (a `Send` method, etc).

Now we'll inject it. In `ConfigureServices` method, add this at the bottom: `services.AddTransient<LocalMailService>();`. Now it can be injected tot he Controller constructor.

There are different lifetime options for the custom services:
* AddTransient: created each time they are requested (for stateless services).
* AddScoped: created one time per request.
* AddSingleton: created the first time they are requested, or if you speciy an instance, when ConfigureServices is executed.

Of course, it's better to use an Interface so we can inject any implementation we want: `IMailService`.

Now in `ConfigureServices` we'll use: `services.AddTransient<IMailService, LocalMailService>();`.

If we want to choose between one implementation or another, we can use Compiler Directives (which chooses on compile, depending on the symbol used):

	#if DEBUG
		services.AddTransient<IMailService, LocalMailService>();
	#else
		services.AddTransient<IMailService, CloudMailService>();
	#endif

...although it is more common to use configuration files. Create an `appSettings.json` at the root of the project:

	{
		"mailSettings": {
			"mailToAddress": "admin@mycompany.com",
			"mailFromAddress": "noreply@mycompany.com"
		}
	}

Now, in `Startup.cs`:

	public class Startup
	{
		public static IConfigurationRoot Configuration;
		
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appSettings.json", optional:false, reloadOnChange:true);
			Configuration = builder.Build();
		}
		
		...
	}

To access those configuration variables from any point in the code: `Startup.Configuration["mailSettings:mailToAddress"]`. The casing of the keys isn't important.

But different environments might have different configuration files. Add `appSettings.Production.json` (it will be added as a child of our existing `appSettings.json`). Now:

	public class Startup
	{
		public static IConfigurationRoot Configuration;
		
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appSettings.json", optional:false, reloadOnChange:true)
				.AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
			
			// The order is important. The last specified configuration file wins.
			
			Configuration = builder.Build();
		}
		
		...
	}

Remember that "Development" or "Production" are environment variables (in the project properties), and that you need to restart VisualStudio to see the changes in those environment variables.


## Entity Framework Core

We'll be using "code first".

Add a new folder at the root: \Entities. Add `City.cs`:

	public class City
	{
		// Not really necessary, because 'Id' is Key by convention:
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id {get;set
		[Required]
		[MaxLength(50)]
		public string Name {get;set;}
		[MaxLength(200)]
		public string Description {get;set;}
		public ICollection<PointOfInterest> PointsOfInterest {get;set;}
			= new List<PointOfInterest>();
	}
	
	public class PointOfInterest
	{
		// Not really necessary, because 'Id' is Key by convention:
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id {get;set;}
		[Required]
		[MaxLength(50)]
		public string Name {get;set;}
		[MaxLength(200)]
		public string Description {get;set;}
		
		[ForeignKey("CityId")]
		public City City {get;set;}
		// Not really necessary, it would infer that City.Id is the foreign key:
		public int CityId {get;set;}
	}

Add the NuGet package: Microsoft.EntityFrameworkCore.SqlServer.

Add `CityInfoContext.cs` under \Entities:

	public class CityInfoContext : DbContext
	{
		public DbSet<City> Cities {get;set;}
		public DbSet<PointOfInterest> PointsOfInterest {get;set;}
	}

Now we have to register this Context, in the Startup class, under `ConfigureServices` method. At the end, add: `services.AddDbContext<CityInfoContext>();`.

To specify the connection string, one option is overriding the `OnConfiguring` method in the `CityInfoContext`:

	public class CityInfoContext : DbContext
	{
		public DbSet<City> Cities {get;set;}
		public DbSet<PointOfInterest> PointsOfInterest {get;set;}
		
		public override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("connectionstring");
			base.OnConfiguring(optionsBuilder);
		}
	}

Another way is overriding the constructor, so we are configuring it at the moment of the creation of the DbContext:

	public class CityInfoContext : DbContext
	{
		public CityInfoContext(DbContextOptions<CityInfoContext> options)
			: base(options)
		{
			// It will create the database the first time it's called:
			Database.EnsureCreated();
		}
	
		public DbSet<City> Cities {get;set;}
		public DbSet<PointOfInterest> PointsOfInterest {get;set;}	
	}

...now in ConfigureServices:

	var connectionString = @"Server=(localdb)\mssqllocaldb;Database=CityInfoDb;Trusted_Connection=True;";
	services.AddDbContext<CityInfoContext>(o => o.UseSqlServer(connectionString));

Use the "sql explorer window" to visualize the database in VisualStudio.


### Migrations

First, we need a tool to create snapshots of the database. It needs new NuGet dependencies: `Microsoft.EntityFramewokCore.Tools`.

Now, in `project.json`, we add:

	"tools": {
		"Microsoft.EntityFramewokCore.Tools": "1.0.0-preview2-final",
		"Microsoft.AspNetCore.Server.IISIntegration.Tools": "1.0.0-preview2-final"
	}

Now you can execute commants in your Package Manager Console: `PM> Add-Migration CityInfoDBInitialMigration`. Now some files will appear under a new folder: \Migrations. If we type `PM> Update-Database`, the changes will be applied to the database.

Under `CityInfoContext.cs`, we can replace `Database.EnsureCreated();` by `Database.Migrate();`. This will look into the non-applied migrations under \Migrations and will execute the missing ones. It keeps track of the executed migrations through a speciffic table in the database (dbo._EFMigrationsHistory).

Every time a change is made to a model, we can add a new migration: `PM> Add-Migration CityInfoDBNewStuffHappened`.


### Safely storing the connection string

#### Development

During development, we'll use `appSettings.json` to store the connection string:

	{
		"mailSettings": {
			"mailToAddress": "admin@mycompany.com",
			"mailFromAddress": "noreply@mycompany.com"
		},
		"connectionStrings": {
			"cityInfoDBConnectionString": "Server=(localdb)\mssqllocaldb;Database=CityInfoDb;Trusted_Connection=True;"
		}
	}

Now we can get it like this: `var connectionString = Startup.Configuration["connectionStrings:cityInfoDBConnectionString"];`.


#### Production

For the production, we'll add the Environment Variables as if they were part of `appSettings.json`:

	public class Startup
	{
		public static IConfigurationRoot Configuration;
		
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appSettings.json", optional:false, reloadOnChange:true)
				.AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables(); // <= new line
			
			Configuration = builder.Build();
		}
		
		...
	}

We'll add the connection string as an Environment value, with the same key used in Development:

* Key: "connectionStrings:cityInfoDBConnectionString"
* Value: "Server=(localdb)\mssqllocaldb;Database=CityInfoDb;Trusted_Connection=True;"

This works well in development... but in fact, those environment variables are the same we set up on windows through "system properties" -> "environment variables". In the server, we would set it up through these ones.

The advantage of those environment variables is that they are not submited to version control.

General rule: use `appSettings.json` for non-sensitive data, use environment variables for sensitive data.


#### Seeding the database with data

Add `CityInfoContextExtensions.cs` to the root of the project:

	public static class CityInfoExtensions
	{
		public static void EnsureSeedDataForContext(this CityInfoContext context)
		{
			if(context.Cities.Any())
			return;
			
			var cities = new List<City>()
			{
				new City()
				{
					...
				},
				new City()
				{
					...
				}
			};
			
			context.Cities.AddRange(cities);
			context.SaveChanges();
		}
	}
	
	// Add a new injected parameter to `Configure` in `Startup.cs`:
	public void Configure( ... , CityInfoContext cityInfoContext)
	{
		...
		cityInfoContext.EnsureSeedDataForContext();
		...
	}





## Using Entity Entity Framework Core for Controllers

### The repository pattern

No code duplication, less error-prone code, better testability of the consuming class. But above all, it is agnostic to which persistence method lies underneath.

Under \Services, we create `ICityInfoRepository` (the repository contract):

	public interface ICityInfoRepository
	{
		IEnumerable<City> GetCitites();
		City GetCity(int cityId, bool includePointsOfInterest);
		IEnumerable<PointOfIneterest> GetPointsOfInterestForCity(int cityId);
		PointOfIneterest GetPointOfInterestForCity(int cityId, int pointOfInterestId);
	}

Also under \Services, we create `CityInfoRepository` (the implementation).

	public class CityInfoRepository
	{
		private CityInfoContext _context;
		public CityInfoRepository(CityInfoContext context)
		{
			_context = context;
		}
		
		public IEnumerable<City> GetCitites()
		{
			// `.ToList()` is important, to effectively execute the query:
			return _context.Citites.OrderBy(c => c.Name).ToList();
		}
		
		public City GetCity(int cityId, bool includePointsOfInterest)
		{
			if(includePointsOfInterest)
				reutrn _context.Cities.Include(c => c.PointsOfInterest)
					.Where(c => c.Id == cityId).FirstOrDefault();
			return _context.Where(c => c.Id == cityId).FirstOrDefault();
		}
		
		...
	}

Under `Startup.cs` we need to configure those new services. Ad the end of the `ConfigureServices` method, we'll add:

`services.AddScoped<ICityInfoRepository, CityInfoRepository>();`

For repositories, the best is Scoped lifetime (once per request).


### Using the repository in the controller

	[Route("api/cities")]
	public class CitiesController : Controller
	{
		private ICityInfoRepository _cityInfoRepository;
		public CitiesController(ICityInfoRepository cityInfoRepository)
		{
			_cityInfoRepository = cityInfoRepository;
		}
		
		[HttpGet()]
		public IActionResult GetCitites()
		{
			var cityEntities = _cityInfoRepository.GetCitites();
			// we have to map this list to CityDto... we'll need a new one (described below)
			var results = new List<CityWithoutPointsOfInterestDto>();
			foreach(var cityEntity in cityEntities)
			{
				results.Add(new CityWithoutPointsOfInterestDto
				{
					Id = cityEntity.Id,
					Name = cityEntity.Name,
					Description = cityEntity.Description
				}
			}
			
			return Ok(results);
		}
		
		...
	}
	
	// Under \Models, add:
	public class CityWithoutPointsOfInterestDto
	{
		public int Id {get;set;}
		public string Name {get;set;}
		public string Description {get;set;}
	}
	

To call this, we need to add the parameters to the URL:
http://localhost:1028/api/cities/1?includePointsOfInterest=true

If it had multiple parameters, we use `&`:
http://localhost:1028/api/cities/1?includePointsOfInterest=true&anotherParameter=itsValue


Now, for the GetCity Action:

	[HttpGet("{id}")]
	public IActionResult GetCity(int id, bool includePointsOfInterest)
	{
		var cityEntity = _cityInfoRepository.GetCity(id, includePointsOfInterest);
		
		// And now return either:
		// - CityDto
		// - CityWithoutPointsOfInterestDto
		// (depending on the value of includePointsOfInterest)	
	}


### AutoMapper

Download AutoMapper with NuGet.

In `Startup.cs`, in the `Configure` method (before app.UseMvc), add: 

	AutoMapper.Mapper.Initialize(cfg =>
	{
		cfg.CreateMap<Entities.City, Models.CityWithoutPointsOfInterestDto>();
		cfg.CreateMap<Entities.City, Models.CityDto>(); // this won't work without (*)
		cfg.CreateMap<Entities.PointOfIneterest, Models.PointOfIneterestDto>(); // (*)
	});

Automapper maps from one to another, ignoring the properties that don't match (by default).

Now we can map it in the controller like so:

	[HttpGet()]
	public IActionResult GetCitites()
	{
		var cityEntities = _cityInfoRepository.GetCitites();
		var results = Mapper.Map(IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities);
		return Ok(results);
	}

When creating a new city, we will also use a mapping between `Entities.City` and `CityForCreationDto`.
	


