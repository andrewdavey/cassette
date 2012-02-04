NOTE: The information in this post is based on a pre-beta release of Cassette 1.1. This is a work in progress. - [@ctcliff](http://twitter.com/ctcliff)

# Managing large JavaScript applications with Cassette.

This post will explain how to manage a Backbone.js application using [Cassette](http://getcassette.net), an asset bundling library for .NET. We'll 
use the popular [Todos](http://documentcloud.github.com/backbone/examples/todos/index.html) application 
by [Jérôme Gravel-Niquet](http://jgn.me/) as a starting point for our example, and I'll demonstrate how to leverage Cassette for 
dependency management, combination and minification of assets, font embedding, image embedding, and compilation of JavaScript templates.

Our example application will make use of several open source projects. I recommend you gain a basic understanding of these before you
begin this tutorial:

- [Backbone.js](cumentcloud.github.com/backbone/), [Underscore.js](http://documentcloud.github.com/underscore/) - JavaScript application framework and utilities
- [jQuery](http://jquery.com/) - DOM manipulation and Ajax library
- [Hogan.js](http://twitter.github.com/hogan.js/) - Compilation and rendering of Mustache templates

## Setup

TODO: Describe file system here.

## Configuring Cassette

Cassette allows you to separate your assets into separate bundles for JavaScript, CSS and HTML templates. These bundles 
are defined in the `CassetteConfiguration.cs` class.

```cs
public void Configure(BundleCollection bundles, CassetteSettings settings)
{
    // Bundle definitions go here.
}
```

Let's add some bundles to the configuration class. First, the CSS:

```cs
bundles.Add<StylesheetBundle>(
    "styles/todos",
    (bundle) => bundle.Processor = new StylesheetPipeline()
        .EmbedImages(path => path.Contains("/embed/"))
        .EmbedFonts(path => path.Contains("/embed/"))
);
```

In this bundle, we've specified that all the stylesheets in `styles/todos` should be included. Additionally, we've told Cassette to
embed both fonts and images. The methods `EmbedImages` and `EmbedFonts` accept an optional anonymous function that allows you to specify
which images should be embedded.

Now we'll add the JavaScript bundles:

```cs
bundles.Add<ScriptBundle>(
    "scripts/lib"
);
bundles.Add<ScriptBundle>(
    "scripts/todos"
);
```

Here we've defined two bundles, one for library code and one for our app-specific code.

Finally, we'll create a bundle for our JavaScript templates:

```cs
bundles.Add<HtmlTemplateBundle>(
    "scripts/todos/template",
    (bundle) => bundle.Processor = new HoganPipeline()
    {
        Namespace = "JST"
    }
);
```

This bundle will locate all the template files located in `scripts/todos/template`.
 using Cassette's `HoganPipeline`. This pipeline will find Mustache templates located in the `scripts/todos/template` directory, compile them with Hogan.js and serve them as a single cacheable JavaScript file. Each template will be compiled into a JavaScript function and added to a globally accessible object we've named `JST`. All we need to do to render a template on the client is call `JST['view-name'].render(data)`.

## The View

Our application will be rendered from a single Razor view.

```html
@inherits WebViewPage<List<Todo>>
<!doctype html>
<html>
<head>
    
    @* Reference the application *@
    @{ Bundles.Reference("scripts/todos"); }
    
    @* Render assets *@
    @Bundles.RenderStylesheets()
    @Bundles.RenderScripts()
    @Bundles.RenderHtmlTemplates()
    
    @* Bootstrap *@
    <script>
        todos.reset(@Html.Raw(JsonConvert.SerializeObject(Model, Newtonsoft.Json.Formatting.None)));
    </script>
    
</head>
<body></body>
</html>
```

