# Contributing #

Thanks for showing interesting in contributing to Cassette! We welcome any additions, but please make sure you read the below notes to avoid any headaches.

## Building ##

We use VS 2010 to build, which means you must be using MSBuild 4.0.

## Resharper ##

It's recommended to use ReSharper but not required; you can learn more about getting XUnit support in R# here: http://xunitcontrib.codeplex.com/wikipage?title=ReSharperSupport&referringTitle=Home

## .NET 3.5 ##

Read about how Cassette [supports .NET 3.5](contributing/fx35.md).

## Nuget Packaging ##

We use `nuspec` files to manage Nuget packing. Make sure the `.nuspec` and `.symbols.nuspec` files stay in-sync. They all should be pointing to the Cassette build directory (`build/bin/<framework>`).

For any new Nuget packages, add them to `build.xml`.