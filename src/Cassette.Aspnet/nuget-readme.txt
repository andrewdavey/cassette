Cassette for ASP.NET has been installed

A file called CassetteConfiguration.cs has been added to your project.
Edit this file to tell Cassette which scripts, stylesheets, etc to bundle.

For documentation, visit http://getcassette.net/


Using MVC areas?
To use the Cassette.Views.Bundles static helper class in your area views, please add the Cassette.Views namespace to each area's Views\Web.config.
i.e.
<configuration>
    ...
    <system.web.webPages.razor>
        <pages>
            <namespaces>
                ...
                <add namespace="Cassette.Views"/>
                ...
            </namespaces>
        </pages>
    </system.web.webPages.razor>
    ...
</configuration