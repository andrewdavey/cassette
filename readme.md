# Cassette

Cassette's website: [getcassette.net](http://getcassette.net).

Web applications today are using more JavaScript than ever. As a result, structuring these files is becoming a problem. You wouldn't put all your C# classes within a single .cs file, so why do that with JavaScript?

Creating lots of smaller .js files is good development practice. However, downloading 100 individual files will make YSlow very unhappy! Better to concatenate and minify the files into logical "bundles" for use in production.

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
* Compile CoffeeScript & LESS (in both debug and release modes).
* Build modules at runtime and cache in isolated storage.
  No complex build tooling required and medium-trust is fully supported.

Cassette does all this [and more](http://getcassette.net/benefits)!

## Install into your web application using Nuget ##

    Install-Package Cassette.Web

## Documentation ##

Check out the [docs](http://getcassette.net/documentation/getting-started) on the website for help getting started.

## Support and Discussion ##

[Join the Discussion Group](http://groups.google.com/group/cassette) to ask questions and learn about new features.

If you have questions, let me know here: [@getcassette](http://twitter.com/getcassette).

[Commercial support](http://getcassette.net/support) is also available if you need it.

## Open Source License ##

Cassette is free software, distributed under the [MIT License](https://raw.github.com/andrewdavey/cassette/master/license.txt)

Copyright (c) 2012 Andrew Davey