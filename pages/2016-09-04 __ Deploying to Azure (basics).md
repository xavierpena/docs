# Deploying to Azure (basics)


## Info

From the pluralsight.com course:

"Building a Site with Bootstrap, AngularJS, ASP.NET, EF and Azure"
by Shawn Wildermuth

Section: Deploying to Azure

## Instance: Azure Websites

Individual in the cloud with Microsoft Service.

New -> Compute -> Web Site -> Quick Create
* URL: just an azure id for your project
* Region: West Europe

http/https are both available (except for the free tier)

## Configure

if you go to your_project_id -> Configure:
* .NET version
* SSL certificates
* Turn on Application Logging at "Error logging level" (it is turned on for 12h)
* App settings (key/value, as in app.config ? ....)
* ...

Finally: "Save"


## Push project to Azure

Visual Studio -> Publish

In Azure we can click on "Download the publish profile". We can import it from the Publish window in Visual Studio.


## Instance: Database

New -> Data Services -> SQL Database -> Custom Create

Once it is created, go back to the Web Site instance -> Linked Resources -> Link an existing resource -> SQL Database

Under Configuration, a new "connection string" was created. If we rename this connection string and replace it with the name that our project had for that connection string, this is enough to make it work. 


## Use your own domain

(In the Scale tab we can change features for this website)

Web Site -> Configure -> Manage domains

First we need to verify that the "domain is ours". So in your domain provider (GoDaddy, Namecheap, etc):
* Modify domain
* Host name: awverify
* IP address/URL: awverify.my_website_id.azurewebsites.net
* Record type: CNAME (Alias)
* Save changes

Once it is verified (the domain name propagation can take minutes), Azure allows us to link the Web Site to our custom domain.

Now Azure gives us the IP address of the website. Go back to your domain provider and put:
* Host name: @
* IP address/URL: <said IP address>
* Record type: A (Address)
* Save changes

Also: redirect the www =>
* Host name: wwww
* IP address/URL: my_website_id.azurewebsites.net
* Record type: CNAME (Alias)

Then save changes.


## Manage db instance

From yor my_db_id dashboard:

Manage allowed IP addresses. Add either "current IP address", or the IP address from which you are trying to access remotely (if you want to connect to the DB from a custom code etc).

To connect to the DB throug code: "Show connection strings".


## Other functionalities for the website

### See logs

Dashboard -> FTP diagnostic logs

User: see "DEPLOYMENT / FTP USER"
Pwd: the password of your my_azure_id instance

### Deploy from source control

Dashboard -> Set up deployment from source control

Although it's easier to use the Visual Studio deployment tool. Also: using version control can mess up with the File System Storage (it pushes the whole repository all the time to the instance).

It easier to keep them separated.