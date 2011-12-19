# Cassette
Web applications today are using more JavaScript than ever. As a result, structuring these files is becoming an problem. You would not put all your C# classes within a single .cs file, so why do that with JavaScript?

Creating lots of smaller .js files is good development practice. However, downloading a hundred individual files will make YSlow very unhappy! We must concantenate and minify the files into logical "modules" for use in production.

In ASP.NET there currently exist partial solutions, but nothing handles all the following:

* Parse the dependencies between scripts and correctly order the files.
  Using JavaScript 'reference' comments already gives you VS intellisense, now they also give automatic build dependency ordering!
* View pages AND partial views can reference scripts.
* Layout/master page makes a single "RenderScripts" call to generate all the required script elements.
* Rich Debug-time output.
  Full, individual source scripts are rendered into the HTML. So debugging with tools like FireBug matches one-to-one with your source.
* Efficient Release-time output.
  JavaScript files are concatentated and minified into modules. Each module is versioned using a hash and is very cache friendly.
* No change in view code between debug and release.
* Compile CoffeeScript (in both debug and release modes).
* Build modules at runtime and cache in isolated storage.
  No complex build tooling required and medium-trust is fully supported.

Cassette does all this!

## Install into your web application using Nuget

    Install-Package Cassette.Web

This will add the assembly references and Cassette HTTP module and handler to Web.config. 

The head over to the wiki to [Get Started](http://getcassette.net/documentation/getting-started).

If you have questions, please let [@andrewdavey](http://twitter.com/andrewdavey) know.

## Discussion Group ##

[Join the Discussion Group](http://groups.google.com/group/cassette) to ask questions and learn about new features.

## Open Source License ##

Copyright 2011 Andrew Davey

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
