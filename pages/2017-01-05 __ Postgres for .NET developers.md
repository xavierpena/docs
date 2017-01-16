# Postgres for .NET developers


## Info

Course at http://pluralsight.com

"Postgres for .NET Developers"

by Rob Conery


## Starting up


### Setting up PostgreSQL

Link: http://postgresql.org/download

For linux: `sudo apt-get install postgresql`.

After installing, go to:

System Properties -> Advanced -> Environment Variables -> PATH -> Edit -> Add:

";C:\Program Files\PostgreSQL\9.4\bin" (or whatever your version of PostgreSQL is)

As a workbench, use "pgAdmin III" (which is installed with Postgres). Connect to the localhosts server, using the superuser password you used during the installation.

Go to "Login Roles" -> "New login role", and add your new user to the superuser group (with all access granted).


### Some basic psql commands

Open PowerShell. Typing "psql" alone won't work because the command needs a db name (or it infers that the db name is the same as your username). So we'll use the system database "postgres":

> psql postgres

See contents of the database with `\d`.

Describe tables: `\dt`.

Describe <tablename>: `\dt <tablename>`.

`insert into <tablename> values ("some test value");`. The command line won't execute the command unless it's ended with ";" (that is: you can keep enter and add new lines, the command is not executed yet).

Then you can `select * from <tablename>;`. It works well with small sets of data. There are other tools such as `psqlrc` that allow you to work with large datasets.

Quit: `\q`.


## Basic functionalities

### A Simple Users Table

	drop table if exists users;
	
	create table users(
		id serial primary key not null,
		email varchar(255) unique not null,
		first varchar(50),
		last varchar(50),
		created_at timestamptz not null default now()
	);
	
	insert into users(email, first, last)
	values('test@test.com', 'Rob', 'Conery');
	
	select * from users;

It's worth it to memorize this line: `id serial primary key not null`.

To modify the table (in this case we'll change the `user_id` column from `bigint` to `int`):

	alter table users
		alter user_id type int;

A rename:

	alter table users
		rename created_at to registered_at;


### Creating a view

Views are saved within the schema.

	drop view if exists pending_users;
	
	create view pending_users as
	select * from users where status = 'pending';
	
	select * from pending_users;


### Functions

Functions are also saved within the schema.

This function will:
* Create a random number between 0 and 1
* Parse it to text
* Hash with md5
* Take the first 'len' chars

	create or replace function random_string(len int) returns text as
	$$
	select substring(md5(random()::text),0,len) as result;
	$$
	LANGUAGE SQL;


### Full text search

If you define a column `search_field tsvector`. `tsvector` is a datatype that allows you to save the search in the search index right in the body of your table.

Now you can use the function `to_tsvector()` in your selects:

> select to_tsvector(email || first || last) from users;

The result of that would be `'test@test.comrobconery':1`.

If you create a trigger:

	create trigger users_search_update_refresh
	before insert or update on users
	for each row execute procedure
	tsvector_update_trigger(search_field, 'pg_catalog.english', email, first, last)

As a result of that, the `search_field` is `'coneri':3 'rob':2, 'test@test.com':1`.

	-- Searches that will work:
	
	select * from membership.users
	where search_field @@ to_tsquery('rob');
	
	select * from membership.users
	where search_field @@ to_tsquery('con:*');
	
	select * from membership.users
	where search_field @@ to_tsquery('test@:*');
	
	select * from membership.users
	where search_field @@ to_tsquery('rob & conery');
	
	select * from membership.users
	where search_field @@ to_tsquery('rob & ! somethingelse');

You don't need a dedicated search field (`search_field`). You can do it on the fly:

	select * from membership.users
	where to_tsvector(concat(email, ' ', first, ' ', last)) @@ to_squery('rob & con:*');


###  (some text) 

When you use this kind of notation, the text you put in between doesn't care if it has quotes / double quotes / whatever. This is why we use it when defining functions.

We can also use it in a select:

> select some weird" word'sss::text;

So we could use:

	select * from membership.users
	where to_tsvector($$email, ' ', first, ' ', last)) @@ to_squery('rob & con:*');


### Relationships

	create table users(
		id serial primary key not null,
		email varchar(255) unique not null,
		first varchar(50),
		last varchar(50),
		created_at timestamptz not null default now()
	);
	
	create table roles(
		id serial primary key not null,
		name varchar(50)
	);
	
	create table users_roles(
		user_id int not null references users(id) on delete cascade,
		role_id int not null references roles(id) on delete cascade,
		primary key(user_id, role_id) 
	);
	



## Advanced functionalities

### Sample data load

http://postgresqltutorial.com/postgresql-sample-database

Download "DVD Rental Sample Database".

Command line:

	> createdb dvdrental`
	> pg_restore -U postgres -d dvdrental C:\temp\dvdrental.tar`
	
	Now access to the database:
	> psql dvdrental
	
	Show tables by blocks:
	> \dt
	
	> select * from films limit 100;


### Explain

`explain select * from payment;`

Will return info about how the query is executed, which can be useful to optimize the queries.


### Working with dates

`select * from payment
where payment_date BETWEEN '2007-03-01' AND '2007-04-01';`

date_gt: "date greater than"

`select * from payment
where date_gt(payment_date, '2007-04-01'::date);`

`select
	amount,
	payment_date,
	date_part('year', payment_date) as year
from payment;`


### Generate series

100 rows from 1 to 100:
`select generate_series(1,100);`

From 5 to 5:
`select generate_series(0,100,5);`

Reverse:
`select generate_series(100,0,-1);`

Use it as a function:
`select x, md5(x) from
generate_series(100, 0, -1) as f(x);`

Use it with dates:
`select x from
generate_series('2001-10-01'::timestamp, '2002-10-01'::timestamp, '10 days') as f(x);`

Combine it with a select, to select 100 random ids:
`select * from payment
where payment_id IN(
	select trunc(random() * 1000)
	from generate_series(1,100)
);`

Create a function, parametrizing the number of random numbers:
	create or replace function random_payments(counter int) returns setof payment as 
	$$
		select * from payment
		where payment_id IN(
			select trunc(random() * 1000)
			from generate_series(1,counter)
		)
	$$ language sql;

Use it like this:
`select * from random_payments(55);`

We can use a diferent language to declare the function and make it more accurate:
	create or replace function random_payments(counter int) returns setof payment as 
	$$
	DECLARE
		start_id int;
		end_id int;
	BEGIN
		select min(payment_id) from payment into start_id;
		select max(payment_id) from payment into end_id;
		return query(
			select * from payment
			where payment_id IN(
				select trunc(random() * (end_id - start_id) + start_id)
				from generate_series(1,counter)
			)
		);
	END
	$$ language plpgsql;


### Common table expressions

`select distinct first_name from autor;`

`select count(1), first_name
from actor
group by first_name
order by count(1) desc
;`

A different way:

	with actor_rollup as (
		select count(1) as name_count, first_name
		from actor
		group by first_name
	)
	select * from actor_rollup order by name_count desc;


### The sales view

Consider this query:

	select title,description,length,ratign,payment.amount,payment_date
	from films
	inner join inventory on inventory.film_id = film.film_id
	inner join rental on rental.inventory_id = inventory.inventory_id
	inner join payment on payment.rental_id = rental.rental_id;

You can enhance it like so:

	create view raw_sales as
	select title,description,length,ratign,payment.amount,payment_date
	date_part('quarter', payment_date) as quarter,
	date_part('month', payment_date) as month,
	date_part('year', payment_date) as year,
	concat('Q', date_part('quarter', payment)::text, '-', date_part('year', payment_date)::text) as qyear,
	cash_words(amount::money) as spelling_it_out,
	to_tsvector(concat(title,description)) as search_field
	from films
	inner join inventory on inventory.film_id = film.film_id
	inner join rental on rental.inventory_id = inventory.inventory_id
	inner join payment on payment.rental_id = rental.rental_id;
	
	select * from raw_sales;


### Partitions

??


### Sales by quarter

Consider this query:

	select title,quarter,year,sum(amount)
	from raw_sales
	group by title,quarter,year
	order by title;

This is speciffic to Postgress: everything that appears in the SELECT must also appear in the GROUP BY (thus the `group by group by title,quarter,year` part).

If we want to know the % of sales for each title per quarter:

	select distinct title,
	sum(amount) over (partition by title,qyear) as "Quarterly Sales",
	sum(amount) over (partition by qyear)
	sum(amount) over (partition by title,qyear)/sum(amount) over (partition by qyear) * 100 as "Percent of Total Quarter"
	from raw_sales
	order by title;


### Sales Queries

(skipped)


### Sales Queries With Full Text

(skipped)


## Postgres and Visual Studio

### dotConnect: visualizing the database in Visual Studio

Download `dotConnect Express for PostgreSQL` via "Extensions and Updates".

Go to "Server Eplorer", right click on Data Connections -> Add Connection

On "Data Source", select "Change". Select "PostgreSQL server". Now fill the parameters of your server (localhost, user, password...).


### Postgres and and Entity Framework

Get `npgsql for Entity Framework` through NuGet.

Under `App.config`:

	<providers>
		...
		<provider invariantName="Npgsql" type="Npgsql.NpgsqlServices, Npgsql.EntityFramework" />
	</providers>
	
	// Also add:
	
	<system.data>
		<DbProviderFactories>
			<add name="Npgsql Data Provider"
				invariant="Npgsql"
				description="Data Provider for PostgreSQL"
				type="Npgsql.NpgsqlFactory, Npgsql" />
			</DbProviderFactories>
	</system.data>
	<connectionStrings>
		<add name="dvds" connectionString="server=localhost;user id=rob;password=password;database=dvdrental" providerName="Npgsql" />
	</connectionStrings>

Now we can create some code:

	// Entity framework forces you to use a key, and the external relations of the table
	[Table("film",Schema="public")]
	public class Film
	{
		[Column("film_id")]
		[Key]
		public int ID {get;set;}
		
		[Column("title")]
		public string Title {get;set;}
	}
	
	public class DB : DbContext
	{
		// "dvds" is the name of the connection string:
		public DB() : base("dvds"){}
		public DbSet<Film> Fimls {get;set;}
	}
	
	class Program
	{
		static void Main(string[] args)
		{
			var db = new DB();
			foreach(var film in db.Films)
			{
				Console.WriteLine(film.Title);
			}
			Console.Read();
		}
	}

(Note: I skipped the other videos this section)


## Simple test-based workflow

You can find the code here:

https://github.com/robconery/pg-dvdrental

Create a new project called `Tests`. Add two directories:
* \Scripts
* \Helpers

\Scripts contains:
* data.sql
* functions.sql
* schema.sql

\Helpers contains `Loader.cs`, which loads all the scripts files, and finally executes them.

Then we can create a new test class, `UserTests.cs`:

	public class UserTests
	{
		ICommandRunner db;
		User user;
		public UserTests()
		{
			Helpers.Loader.ReloadDb();
			db = new CommandRunner("dvds");
			user = db.ExecuteSingle<User>("select * from users");
		}
		
		[Fact]
		public void UserShouldExist()
		{		
			Assert.NotNull(user);
		}
	}


## NoSQL

### JSON with Postgres

`select row_to_json(film) from film;`

Returns each row with the following format:

	{
		"film_id":133,
		"title:"Chamber italian",
		"description":"..."
		...
	}

We can transform it to json binary (jsonb) like so:

`select row_to_json(film)::jsonb from film;`

We can create a NoSQL talbe like so:

`create table film_docs(data jsonb);`

`insert into film_docs(data)
select row_to_json(film)::jsonb from film;`

Now this is pure jsonb for each row:
`select * from film_docs;`

And we can query its inner json!
`select 
	(data -> 'title') as Title, (data -> 'length') as Length 
from film_docs;`

This returns the strings like `"Hi I'm a string"`. But if you do `(data ->> 'title')` (double greater-than), it returns `Hi I'm a string` without the double quotes.

There are other operators that can be used: `->`,`->>`,`#>`,`#>>`.

`select 
	(data ->> 'title') as Title, (data -> 'length') as Length 
from film_docs
where (data ->> 'title') = 'Chamber Italian'
;`

Other options:

Check for existance (whether there is a key 'title' with a value of 'Chamber Italian'):
`where data -> 'title' ? 'Chamber Italian'`

Where data contains (using the "contains" operator). Check wheter the data contains this key and value: 
`where data @> {"title" : "Chamber Italian"}`

We can create indexes:

`create index on film_ocs using GIN(data);`

Now if we use "@>" we take advantage of the indexing.


### Saving a user document

	create table user_documents(
		id biging primary key not null default to id_generator(),
		body jsonb,
		search_field tsvector
	);

To save it in c#:

`var serialized = JsonConvert.SerializeObject(user);`

And then execute a query with:

`"insert into user_documents (body, search_field)
values(@0, search_field=to_tsvector(concat(@2,@3,@4)))",
serialized,
user.email,
user.first,
user.last,
user.ID`

...or to update it:

`"update user_documents set body=@0, search_field=to_tsvector(concat(@2,@3,@4)), where id=@1",
serialized,
user.email,
user.first,
user.last,
user.ID`


### Reading a user document

	var record = <get record from db ...>
	var result = JsonConvert.DeserializeObject<User>(record.body);


(skipped "Authenticating with users")


