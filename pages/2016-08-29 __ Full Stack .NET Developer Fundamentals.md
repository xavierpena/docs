# Full Stack .NET Developer Fundamentals


## Info

[ Pluralsight course, with Mosh Hamedani](https://app.pluralsight.com/library/courses/full-stack-dot-net-developer-fundamentals/table-of-contents)


## Userful shortcuts

- Duplicate line: Ctrl+D
- Build: Ctrl+Shift+B
- Select word: Ctrl+W
- Move between windows: Ctrl+Tab
- Add new item: Ctrl+Shit+A

Other:
- ctor: create class constructor
- prop: create get-set property


## Setting it up

Download Extensions and Updates:
- Productivity Power Tools
- Web Essentials

To automatically remove "unused usings" on save (with Productivity Power Tools):

Tools => Options => Productivity Power Tools => PowerCommands => Remove and Sort usings on save / auto format on save


## User cases

Detail the "use cases" (or "user stories").

Put them in the "backlog" of your Visual Studio online.

Detail the dependencies between features. The initial dependency must be implemented first. Order all of them to show the order in which they need to be implemented.

Identify the core use cases (for example: the ones that change the state of the application in a significant way). Once you have indentified them, select some flow of dependencies that have those core dependencies and works as a "full story". We will start building this application skeleton.


## Code-first migrations

Enable migrations: go to NuGet package manager, write `enable-migrations`.

Workflow:
* Create the model (in models)
* Open Models/IdentityModels.cs
* Under ApplicationDbContext, add: `public DbSet<ModelName> ModelNames {get;set;}`
* Open package manager console
* `add-migration ArbitraryMigrationName` (or `-force` to overwrite if the `ArbitraryMigrationName` already existed)
* `Update-database`

To add specifications to each field (such as "not null") we use Attributes (such as "[Required]") over the property. The migration will apply this to the DB.

Create a sql script:

* Open package manager console
* `add-migration ArbitraryMigrationName`
* Open the DbMigration class (which will be empty)
* Under Up() => `Sql("INSERT INTO table (Id, Name) VALUES (1, "blah")")`
* Under Down(), put the statements that will revert the database to the previous state: `Sql("DELETE  FROM table WHERE Id=1`
* execute `update-database`

## Adding a basic MVC item

This section will implement a basic part of a CRUD.

* \Controllers => add empty MVC5 controller, with the name "ModelLabel(s)"
* \Views\ModelLabel(s) => Add View => "ActionLabel"

Razor syntax for a link: `@Html.ActionLink("Link text", "ActionName", "ControllerName")`

In your view:

At the top:
@model ModelClassName

Form using bootstrap:
	<form>
		<div class="form-group">
			<label>
			<input class="form-control">
		</div>
		...
		<button type="submit" class="btn btn-primary">Save</button>
	</form>

Concrete example:

	<form>
		<div class="form-group">
			@Html.LabelFor(m => m.PropertyName)
			@Html.TextBoxFor(m => m.PropertyName, new { @class = "form-control"})
		</div>
		...
	</form>

When the interaction in the View does not exactly correspond to the property in the model (for example: DateTime in the model, but in the view we ask for Date and Time separately), in this case we use a ViewModel: \ViewModels => add class `ModelnameFormViewModel`. Almost all of its properties will be strings, since all the user writes in the form are strings.

Now replace `@model ModelClassName` to `@model ModelnameFormViewModel`.

Html dropdown list: create a `IEnumerableSource` as a property in the ViewModel.

	@Html.LabelFor(m => m.PropertyName)
	@Html.DropDownListFor(m => m.PropertyName, new SelectList(Model.IEnumerableSource, "Id", "Name"), "", new { @class = "form-control"})
	// In `<option value="xxx">yyy</option>`
	// xxx = Model.IEnumerableSource[elementIndex].Id
	// yyy = Model.IEnumerableSource[elementIndex].Name

Populating the IEnumerableSource of the ViewModel is a job for the controller.

	public class ModelnameController : Controller
	{
		private readonly ApplicationDbContext _context;
		
		public ModelnameController()
		{
			_context = new ApplicationDbContext();
		}
		
		public ActionResult Actionname()
		{
			var viewModel = new MyViewModel
			{
				IEnumerableSource = _context.Modelname.ToList()
			}
			return View(viewModel);
		}
	}


To limit access to authenticated users, put `[Authorize]` on the top of the ActionResult in the Controller.

To redirect the form to the correct action:

	// In the view:
	@using(Html.BeginForm("ActionName", "ControllerName"))
	{
		<div class="form-group">
			@Html.LabelFor(m => m.PropertyName)
			@Html.TextBoxFor(m => m.PropertyName, new { @class = "form-control"})
		</div>
		...
	}
	
	...
	
	// In the controller:
	[Authorize]
	[HttpPost]
	public ActionResult ActionName(ModelnameFormViewModel viewModel)
	{
		var modelname = new Modelname
		{
			PropertyName = viewModel.PropertyName,
			PropertyName = viewModel.PropertyName
		}
		_context.Modelname.Add(modelname);
		_context.SaveChanges();
		
		return RedirectToAction("Index", "Home");
	}

"Navigation properties": properties of a model that contain a different model (connected through an id in the database). Ids in MSSQL are strings (a GUID).

In the model, instead of just having `public NavigationObj NavigationProperty { get; set; }`, it is better to have:

	public NavigationObj NavigationProperty { get; set; }
	
	[Required]
	public string NavigationPropertyId { get; set; }

Then, remember to `add-migration migrationName` and `update-database`. The database will change the property names from `NavigationProperty_Id` (which was the default) to `NavigationPropertyId`.

## Implementing validation

	// in the ViewModel:
	public class BlahFormViewModel
	{
		[Required]
		public string Prop1 {get;set;}
		
		[Required]
		public string Prop2 {get;set;}
		
		[Required]
		public string Prop3 {get;set;}
	}
	
	// ...and in the view:
	
	@using(Html.BeginForm("ActionName", "ControllerName"))
	{
		<div class="form-group">
			@Html.LabelFor(m => m.PropertyName)
			@Html.TextBoxFor(m => m.PropertyName, new { @class = "form-control"})
			@Html.ValidationMessageFor(m => m.PropertyName)
		</div>
		...
	}
	
	// ...and in the controller:
	public ActionResult Actionname(BlahFormViewModel viewModel)
	{
		if(!ModelState.IsValid)
			return View("Actionname", viewModel);
		
		// the rest of the code
	}
	

Custom validation (so we can add `[FutureDate]` on top of the validated property):

	public class FutureDate : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			// Convert.ToString(value) instead of value.ToString(), because value could be null:
			DateTime dateTime;
			var isValid = DateTime.TryParseExact(Convert.ToString(value), "d MMM yyyy", CurrentInfo.CurrentCulture, DateTimeStyles.None, dateTime);
			
			return (isValid && dateTime > DateTime.Now);
		}
	}

Enable client-side validation, so we don't need to reload the page if there is an error in the validation.

The bundle must be registered under `RegisterBundles()` (it is by default).

Then, at the bottom of the .cshtml view, add:

	// Under "section scripts" to ensure it is loaded after the default jquery bundle (which is set in the main layout)
	@section scripts
	{
		@Scripts.Render("~/bundles/jqueryval");
	}


## Common vulnerabilities

* SQL injection: nefarious SQL statements are inserted into an entry field for execution
* XSS: Cross-site scripting (XSS) is a type of computer security vulnerability that enables attackers to inject client-side scripts into web pages viewed by other users. 
* CSRF: Cross-site request forgery, also known as one-click attack or session riding and abbreviated as CSRF (sometimes pronounced sea-surf) or XSRF, is a type of malicious exploit of a website where unauthorized commands are transmitted from a user that the website trusts. Unlike cross-site scripting (XSS), which exploits the trust a user has for a particular site, CSRF exploits the trust that a site has in a user's browser.

How to prevent XSS: scape special characters in the input.

How to prevent CSRF:

	// In the view:
	using(Html.BeginForm())
	{
		@Html.AntiForgeryToken()
	}
	
	// And in the controller:
	[ValidateAntiForgeryToken]
	public ActionResult Actionname(BlahFormViewModel viewModel)
	{
		...
	}


## Design / Look and feel

Test the style with Chrome Dev Tools. This way you'll know which element is determining the style as well.

\Content\Site.css

	/* Bootstrap Overrides */
	.navbar-inverse {
		background-color: #ff4342;
		border-color: #ff4342;
	}

Fonts: go to google.com/fonts. Select the fonts. After clicking "Use", you'll have to select "normal" and "bold". There are instructions abou how to use them in your website: 

	// In _Layout.cshtml:
	<link href='htpps://fonts.googleapis.com/ ...'>
	@Styles.Render("~/Content/css")
	...
	
	font-family: 'Open Sans', sans-serif;

Go to `\Content\bootstrap.css`. Select all the instances of the font that you want to replace. Copy those css elements, and paste them into `\Content\Site.css` with the new font (which will override the default bootstrap style). We can keep the other fonts after the new one (in this case Lato) as a "fallback mechanism": `font-family: Lato, "Helvetica Neue", Helvetica, Arial, sans-serif;` (instead of the default `font-family: "Helvetica Neue", Helvetica, Arial, sans-serif;`).


## Usability best practices

Labels: aligned to the right (next to the field) if vertical space is an issue. Otherwise: on the top of the field.

Input fields:
- Reduce the number of fields
- Avoid optional fields
- Separate mandatory and optional fields. Mandatory: "*" in red. I fall mandatory: put it on the top: <p class="alert alert-info">All fields are <strong>mandatory</storng></p>.
- Group related fields
- Specify the format: @Html.TextBoxFor(m => m.PropertyName, new { @class = "form-control", placeholder = "Eg 1 Jan 2016"})
- Set focus on the first field: @Html.TextBoxFor(m => m.PropertyName, new { @class = "form-control", autofocus = "autofocus"})

Actions ("Save" / "Cancel" etc):
* Each form should have a primary action ("Save")
* Avoid secondary actions if possible ("Cancel")
* Otherwise, visually separate them
* Align primary actions ("Save") with input fields, vertically

Validation:
* Provide clear validation messages
* Use red to indicate errors
* Use green to indicate success
* Provide smart defaults (location / calculate value ...)


## Extending ASP.NET Identity Users

In `Models\ApplicationUser.cs`, we extend `ApplicationUser` ((which derives from the base class `IdentityUser`), and add the extra fields. User [Required], [StringLength(100)] etc. Remember, these attributes are for the database only. The attributes for data validation are in the ViewModel, not in the model.

Then we need to update the database: `add-migration migrationName` => review migrations => `update-database`.

Then we modify the sign-up form: \Views\Account\Register.cshtml => implement the proper razor syntax to add the field.

The html form interacts with the ViewModel, so the new fields need to be added to the ViewModel as well.

Finally in the AccountController, we need to go to the `Register(RegisterViewModel model)` so the new fields in the ViewModel are added to the Model before the Model is inserted into the database.


## CSS Techniques

Organize Style.css with the following order:
* General-level styles
* Boostrap-specific styles
* Page-level styles

When debugging: at the "refresh" icon, "Enable Browser Link" and "Enable CSS Auto-Sync" must be activated.

Web essentians plugin has a feature called "Zencoding":

`div.date` ## Tab = `<div class="date"></div>`

`(div.date>div.month+div.day)+(div.details>span.artist+span.genre)` ## Tab =

	<div class="date">
		<div class="month"></div>
		<div class="day"></div>
	</div>
	<div class="details">
		<span class="artist"></span>
		<span class="genre"></span>
	</div>

span: when you only want to decorate the text.

Relative vs Asbolute position: an element with relative position allows us to absolutely position its children.

	/* This means "li inmediatelly below a className element" */
	.parentClassName > li {
		position: relative;
	}
	
	/* childClassName is under a "relative" element so we can use "absolute" on it */
	.parentClassName > li .childClassName {
		position: absolute;
		top: 0;
		left: 60px;
	}


## Composite keys

	public NavigationObj1 NavigationPropertyObj1 {get;set;}
	public NavigationObj2 NavigationPropertyObj2 {get;set;}
	
	[Key]
	[Column(Order=1)]
	public int NavigationObj1Id {get;set;}
	
	[Key]
	[Column(Order=2)]
	public int NavigationObj2Id {get;set;}
	

We might have problems with the "cascade delete" if the paths between cascading db objects is too complex.

To solve that, in order to use "fluend API" we need to override OnModelCreating. In ApplicationDbContext. This way we can modify the modelBuilder to supply additional stuff to replace the default conventions:

	proctected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Attendance>()
			.HasRequired(a => a.Gig)
			.WithMany()
			.WillCascadeOnDelete(false);
		base.OnModelCreating(modelBuilder);
	}

...and then we need to regenerate the migration (if it was previously generated).


## Implementing a restfull API

This will be done in order to perform AJAX calls to our database.

Copy `GlobalConfiguration.Configure(WebApiConfig.Register)` into `Global.asax.cs` (in the first line of `Application_Start()`).

Add => Controller => Web API 2 Controller

	public ControllernameController : ApiController
	{
		private ApplicationDbContext _context;
		
		public ControllernameController()
		{
			_context = new ApplicationDbContext();
		}
		
		[HttpPost]
		public IHtppActionResult Actionname([FromBody] objType objName)
		{
			// some previous tests
			if(testsFailed)
				return BadRequest("Meaningful error message");
			
			// code to save stuff into the database				
			return Ok();		
		}
	}

You can test the API with the Chrome extension "Postman" ([ link](https://chrome.google.com/webstore/detail/postman/fhbjgbiflinjbdggehcddcbncdddomop)).

Maybe you need to define a header: Header = "content-type" | Value = "application/json"


## Calling the restful ApiController

The convention is to call `baseaddress/api/apicontrollername`.

In the .cshtml, at the bottom:

	@section scripts
	{
		<script>
			$(document).ready(function() {			
				$("my-button-id").click(function(e){
					var button = $(e.target);
					$.post("/api/attendances", { "": button.attr("attribute-id-where-the-info-is-stored") })
					.done(function(){
						button
							.removeClass("btn-default")
							.addClass("btn-info")
							.text("Success!")
					})
					,fail(function(){
						alert("Something went wrong...");
					});
				})
			});
		</script>
	}

"e" is the event, "e.target" is the source of the event (in this case: the button).

The `{ "": }` syntax is because we put [FromBody] in the input of `IHtppActionResult Actionname`. It also can work with `{ objNameInApi : ... }`.

Convention: "Dto" (Data Transfer Object). Like in `ObjnameDto`. They can be in a separate folder called "Dtos". Those Dtos are going to be the input of the IHtppActionResult functions.


## Other

Adding nested properties, use "Include":

	var items = _context.Modelname
		.Where(x => x.Property == propValue)
		.Include(x => x.Nestedproperty1)
		.Include(x => x.Nestedproperty2)
		.ToList();

Views shared by different controllers must be under `\Views\Shared`. Then, in the controller, you can use `return View("Sharedviewname", viewModel)`