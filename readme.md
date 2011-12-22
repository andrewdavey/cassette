# Cassette
Web applications today are using more JavaScript than ever. As a result, structuring these files is becoming an problem. You would not put all your C# classes within a single .cs file, so why do that with JavaScript?

Creating lots of smaller .js files is good development practice. However, downloading a hundred individual files will make YSlow very unhappy! We must concatenate and minify the files into logical "bundles" for use in production.

In ASP.NET there currently exist partial solutions, but nothing handles all the following:

* Parse the dependencies between scripts and correctly order the files.
  Using JavaScript 'reference' comments already gives you VS intellisense, now they also give automatic build dependency ordering!
* View pages AND partial views can reference scripts.
* Layout/master page makes a single "RenderScripts" call to generate all the required script elements.
* Rich Debug-time output.
  Full, individual source scripts are rendered into the HTML. So debugging with tools like FireBug match one-to-one with your source.
* Efficient Release-time output.
  JavaScript files are concatentated and minified into bundles. Each bundle is versioned using a hash and is very cache friendly.
* No change in view code between debug and release.
* Compile CoffeeScript (in both debug and release modes).
* Build modules at runtime and cache in isolated storage.
  No complex build tooling required and medium-trust is fully supported.

Cassette does all this!

## Install into your web application using Nuget

    Install-Package Cassette.Web

The head over to the wiki to [Get Started](http://getcassette.net/documentation/getting-started).

If you have questions, please let [@andrewdavey](http://twitter.com/andrewdavey) know.

## Discussion Group ##

[Join the Discussion Group](http://groups.google.com/group/cassette) to ask questions and learn about new features.

## Open Source License ##

Cassette is free software, distributed under the [MIT License](https://raw.github.com/andrewdavey/cassette/master/license.txt)

Copyright (c) 2011 Andrew Davey