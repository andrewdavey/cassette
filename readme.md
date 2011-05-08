# Knapsack
Web applications are using much more JavaScript. As a result structuring these files is becoming an problem. You would not put all your C# classes within a single .cs file, so why do that with JavaScript?
Creating lots of smaller .js files is good development practice. However, downloading a hundred individual files will make YSlow very unhappy! We must concantenate and minify the files into logical "modules" for use in production.
There currently exist partial solutions, but nothing handles all the following:
* Parse the dependencies between scripts and correctly order the files.
  Using JavaScript 'reference' comments already gives you VS intellisense, now they also give automatic build dependency ordering!
* No change in view code between debug and release.
* View pages AND partial views can reference scripts.
* Layout/master page makes a single "RenderScripts" call to generate all the required script elements.
* Rich Debug-time output.
  Full, individual source scripts are rendered into the HTML. So debugging with tools like FireBug matches one-to-one with your source.
* Efficient Release-time output.
  JavaScript files are concatentated and minified into modules. Each module is versioned using a hash and is very cache friendly.
* Compile CoffeeScript (in both debug and release modes).
* Build modules at runtime and cache in isolated storage.
  No complex build tooling required and medium-trust is fully supported.

## Install into your web application using Nuget
Install-Package Knapsack

This will add the assemblies references and Knapsack HTTP module and handler to Web.config. 
