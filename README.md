## Problem statement ##

Server-side errors are generally easy to deal with - they can be caught and recorded in log files, or trigger email
notifications etc. However, client-side errors that occur in the browser are a different matter - the user may spot
that something has gone wrong, but non-technical users are unlikely to check the console, and the application 
developer will generally be unuware that these errors have occurred.

## The solution ##

There are several excellent third-party solutions to this problem, but they are not free. Sentinel is designed to
provide a very simple open-source solution to this problem.
Currently it only logs errors, although in future there is the potential for it to log more detailed information, such as network requests or UI interactions.

## Usage ##

Include this on every page of your application:

`<script src="https://ydh-watchdog.ydh.nhs.uk/Sentinel/js/sentinel.min.js"></script>`

`Sentinel.init('MY_APPLICATION_NAME');`

You may need to update the CSP for your site accordingly, by adding ydh-watchdog.ydh.nhs.uk to the list of domains
that it allows scripts to be loaded from. 

Any errors will be sent to the Sentinel server, these can be viewed at [https://ydh-watchdog.ydh.nhs.uk/Sentinel](https://ydh-watchdog.ydh.nhs.uk/Sentinel). **Note:** the script always sends errors back to the location where the script was
loaded from, so this doesn't need to be specified separately.

## Vue.js integration ##

There is also a Vue.js integration to catch any errors that occur in the Vue object. Just include the following script
anywhere after the main Sentinel script:

`<script src="https://ydh-watchdog.ydh.nhs.uk/Sentinel/js/sentinel-vue.min.js"></script>`

## Databases ##

The production instance uses the Sentinel database on maildog. Login details for the application are in 
appsettings.Production.json - a copy of this will need to be taken from ydh-watchdog, as it's not in source control.
The database is extremely simple so should work on any relatively recent version of SQL Server.

## Authentication ##

Active directory authentication is used for logins. Additional users can be added on the User Admin page.

## Development ##

To do any development you will need to install Visual Studio 2019 (16.8+). Clone the repository and 
then just open Sentinel.sln from within Visual Studio. You will need to 
grab a copy of the appsettings.*.json files from it's current installation on ydh-watchdog, 
and put these in the Sentinel folder (next to appsettings.json). 

For development purposes you should make your own copy of the database, 
and alter the connection string in appsettings.Development.json accordingly.

You can then test it by running any web application, but instead of including a link
to https://ydh-watchdog.ydh.nhs.uk/Sentinel/js/sentinel.min.js as described above, include a link to your instance
running on localhost, i.e. https://localhost:XXXXX/js/sentinel.min.js

This application targets .NET 5.0, hence the requirement for version 16.8+ of Visual Studio.

As part of the build it runs a minifier on the sentinel.js and sentinel-vue.js scripts (see bundleconfig.json),
although it doesn't bother rebuilding unless a change has been made to the C# code.

## Deployment

The project has been set up with web deployment configured for ydh-watchdog. In order
to use this you will need to ensure that permissions have been set up in IIS:

Click on the Sentinel application in IIS, then on IIS Manager Permissions. 
Click on "Allow User..." to add yourself to the list. Then click the "YDH-WATCHDOG"
root node, then Management Service. Stop the service, add your IP address to the
list and start the service again.

To deploy, right-click on the Sentinel project in Solution Explorer, and select "Publish..."
Ensure "Watchdog" is selected as the publish profile and click Publish. If there is an error,
try clicking Edit, then "Validate Connection", If this works, cancel and deploy again.
If not you may have to fix the permissions as described above.

It could potentially be deployed to other locations, Watchdog uses IIS 10 with the following add-ons:
- Build Tools for Visual Studio 2019
- .NET Core hosting bundle
- WebDeploy

Note that it can't be deployed to a Linux server as the code used for the AD logins is Windows-specific.
