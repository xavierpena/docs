# The joys of case sensitivity

## Introduction

It was my first time deploying a asp.net core app on linux, and I stumbled upon this interesting problem. Just wanted to share it with you as a warning, and to learn more about it from your comments. I've already had other gotchas regarding case sensitivity on linux, such as [this one](http://stackoverflow.com/a/41342705/831138). I guess when developing on a windows machine you just lower one's guard regarding this kind of things.

## The error

Running an asp net core application on Ubuntu, I had the following error:

    MySql.Data.MySqlClient.MySqlException: Table 'myschema.AspNetUsers' doesn't exist

Running `show tables;` on the server's mysql instance, I got:

	+-----------------------+
	| Tables_in_myschema    |
	+-----------------------+
	| __efmigrationshistory |
	| aspnetroleclaims      |
	| aspnetroles           |
	| aspnetuserclaims      |
	| aspnetuserlogins      |
	| aspnetuserroles       |
	| aspnetusers           |
	| aspnetusertokens      |
	| (+ custom tables)     |
	+-----------------------+

<= notice they are all lower case. They were automatically created on Windows and dumped to linux.


One more test:

	mysql> select * from AspNetUsers;
	ERROR 1146 (42S02): Table 'myschema.AspNetUsers' doesn't exist

	mysql> select * from aspnetusers;
	(OK, returns the rows)


## Possible solutions

Maybe I could solve this by deleting the asp.net auto-generated tables and re-creating them on linux (I don't know if they are re-created lower case). But not being able to create a fully working database on my windows machine and then dumping it to linux can be a big hassle... 

I could also create a script that renames all the tables after the dump, but I don't know if the problem goes further (fields in tables also generated with lower-case names on Windows?).

I can also configure mysql with `lower_case_table_names=1` [as explained here](http://stackoverflow.com/questions/6134006/are-table-names-in-mysql-case-sensitive).

My guess is that it's better to attack the source of the problem and [force case-sensitive table names on windows](http://stackoverflow.com/questions/6248735/how-to-force-case-sensitive-table-names): `/my.ini: lower_case_table_names=2`. It is case insensitive by default and I never bothered to change it.


## Applied solution

Renaming the tables solved the issue:

	rename table __efmigrationshistory to __EFMigrationsHistory;
	rename table aspnetroleclaims to AspNetRoleClaims;
	rename table aspnetroles to AspNetRoles;
	rename table aspnetuserclaims to AspNetUserClaims;
	rename table aspnetuserlogins to AspNetUserLogins;
	rename table aspnetuserroles to AspNetUserRoles;
	rename table aspnetusers to AspNetUsers;
	rename table aspnetusertokens to AspNetUserTokens;