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

Any errors will be sent to the Sentinel server, these can be viewed at [https://ydh-watchdog.ydh.nhs.uk/Sentinel](https://ydh-watchdog.ydh.nhs.uk/Sentinel)

## Vue.js integration ##

There is also a Vue.js integration to catch any errors that occur in the Vue object. Just include the following script
anywhere after the main Sentinel script:

`<script src="https://ydh-watchdog.ydh.nhs.uk/Sentinel/js/sentinel-vue.min.js"></script>`


