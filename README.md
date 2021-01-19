## Problem statement ##

Server-side errors are generally easy to deal with - they can be caught and recorded in log files, or trigger email
notifications etc. However, client-side errors that occur in the browser are a different matter - the user may spot
that something has gone wrong, but non-technical users are unlikely to check the console, and the application 
developer will generally be unaware that these errors have occurred.

## The solution ##

There are several excellent third-party solutions to this problem, but they are not free. Sentinel is designed 
to provide a very simple open-source solution to this problem.
Currently it only logs errors, although in future there is the potential for it to log more detailed information, such as network requests or UI interactions.

## Usage ##

Include this on every page of your application:

`<script src="https://[SERVER]/Sentinel/js/sentinel.min.js"></script>`

`Sentinel.init('MY_APPLICATION_NAME');`

You may need to update the CSP for your site accordingly, by adding the name of the server that is hosting Sentinel to the list of domains that it allows scripts to be loaded from. 

Any errors will be sent to the Sentinel server, these can be viewed at [https://[SERVER]/Sentinel](https://[SERVER]/Sentinel). The script always sends errors back to the location where the script was
loaded from, so this doesn't need to be specified separately.

### Un-minification (NEW!) ###

Sometimes an error may occur in a minified JavaScript file. In this case, the stack trace and code view won't be
particularly useful. Sentinel will try to overcome this by un-minifying the stack trace, if possible. In order
for this to be possible, Sentinel requires the source map file (with either a min.map or min.js.map extension) 
and the unminified source (optional - if you want the "View code" page to display the unminified source). 

**You have two options for providing these:**
1. Place the files on the server in the same directory
2. If you don't wish to do this, you can upload the source map and unminified source to Sentinel using the
"Uploads" page. They are saved in the database with the application name and name of the unminified source file.


### Note on cross-domain scripts ###

If an error occurs in a third-party script that is loaded from another domain, for example from a CDN,
the error information won't get populated and you will end up with a generic "script error" and no other information.

To fix this, two things are required - firstly add `crossorigin="anonymous"` to the script tag.
Secondly, the server that serves this script must add the `Access-control-allow-origin: *` header to the response.

The other option is to download the script and serve it locally from the same domain.

## Vue.js integration ##

There is also a Vue.js integration to catch any errors that occur in the Vue object. Just include the 
following script anywhere after the main Sentinel script and Vue have been included:

`<script src="https://[SERVER]/Sentinel/js/sentinel-vue.min.js"></script>`

## How it works ##

The Sentinel script simply overrides window.onerror(), and POSTSs the details of any errors back
to the server. For the Vue integration it overrides VUe.config.errorHandler().

Some work is necessary on the server side to get the errors into a consistent format - for example, errors
generated by the datatables library put the error information in non-standard fields.

## Databases ##

A script to create the database is given in the Database folder. The connection string should be specified
in appsettings.Development.json and appsettings.Production.json (ususally a local database would be used
in development mode). The appsettings files are not in source control, and example is given below.
The database is extremely simple so should work on any relatively recent version of SQL Server (or other
SQL database).)

## Authentication ##

Active directory authentication is used for logins. Additional users can be added on the User Admin page.
Connecting to an on-premesis AD server required some custom code, this can be found in the Authentication
sub-project. These custom classes are then referenced when ASP.NET Identity is setup - see the Startup.CSP
file at around line 43.
This setup could be changed back to use e.g. Azure AD, but that is outside the scope of this document.

## Development ##

To do any development you will need to install Visual Studio 2019 (16.8+). Clone the repository and 
then just open Sentinel.sln from within Visual Studio. You will need to 
create the appsettings.*.json files (see below), 
and put these in the Sentinel folder (next to appsettings.json). 

For development purposes you should make your own copy of the database, 
and alter the connection string in appsettings.Development.json accordingly.

You can then test it by running any web application, but instead of including a link
to https://[SERVER]/Sentinel/js/sentinel.min.js as described above, include a link to your instance
running on localhost, i.e. https://localhost:XXXXX/js/sentinel.min.js

This application targets .NET 5.0, hence the requirement for version 16.8+ of Visual Studio.

As part of the build it runs a minifier on the sentinel.js and sentinel-vue.js scripts (see bundleconfig.json),
although it does *not* rebuild them unless a change has been made to the C# code.

## Deployment

The project is normally deployed to an IIS server using Web deploy. In order
to use this you will need to ensure that permissions have been set up in IIS:

Create an ApplicationPool and Application for Sentinel in IIS.
Next, click on the Sentinel application, then on IIS Manager Permissions. 
Click on "Allow User..." to add yourself to the list. Then click the server
root node, then Management Service. Stop the service, add your IP address to the
list and start the service again.

To deploy, right-click on the Sentinel project in Solution Explorer, and select "Publish..."
You will need to set up a publish profile for web deployment. If there is an error,
try clicking Edit, then "Validate Connection", If this works, cancel and deploy again.
If not you may have to fix the permissions as described above.

Sentinel is currently deployed on a server running IIS 10 with the following add-ons:
- Build Tools for Visual Studio 2019
- .NET Core hosting bundle
- WebDeploy

Note that it can't be deployed to a Linux server as the code used for the AD logins is Windows-specific.

For more information on Web Deploy see https://docs.microsoft.com/en-us/visualstudio/deployment/quickstart-deploy-to-a-web-site?view=vs-2019

## Example appsettings file

Copy this to appsettings.Development.json and appsettings.Production.json (next to appsettings.json)
and edit as required:

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "ApplicationConnection": "Data Source=SERVER\INSTANCE;Initial Catalog=Sentinel;Trusted_connection=true"
  },
  "ApplicationOptions": {
    "SmtpHost": "127.0.0.1",
    "SmtpPort": "25",
    "EmailFrom": "no-reply@test.com",
    "EMailCc": "",
    "EMailBcc": ""
  }
}
