.NET 3.5
========

## Installation ##

1. Copy the DLLs to your site's `bin` directory.
2. Copy the `CassetteConfiguration.cs.pp` file to your project and remove the `pp` extension. Change your namespace.
3. Add the HTTP module and handlers from `web.config.fx35.transform` if they don't already exist to your root `web.config`

For **MVC2**, modify your `~/Views/web.config` and add the `Cassette.Views` namespace (from `web.config.mvc2.transform`).

## Nuget ##

This is a temporary installation procedure for .NET 3.5 users until Nuget supports targeted dependencies. Want it faster? Vote for it! 

http://nuget.codeplex.com/workitem/697

## Troubleshooting ##

* Cassette is returning HTTP 404 for URLs and when browsing to `_cassette`.

Make sure you have a reference to `System.Web.Routing` and your `web.config` has the `UrlRoutingModule` and handler specified. This is configured already for MVC2 applications but may not be for Web Forms applications. For reference, see: http://msdn.microsoft.com/en-us/magazine/dd347546.aspx#id0070014

* It still doesn't seem to find my bundles.

Make sure `runAllManagedModulesForAllRequests` is set to `true` for your `web.config` `system.webServer/httpModules` element.

* Cassette is not processing my SASS files.

SASS is only supported in .NET 4.