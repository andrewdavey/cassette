NOTE: The information in this post is based on a pre-beta release of Cassette 1.1. This is a work in progress. - [@ctcliff](http://twitter.com/ctcliff)

# Managing Backbone.js applications with Cassette.

This post will explain how to manage a Backbone.js application using [Cassette](http://getcassette.net), an asset bundling library for .NET. We'll 
use the popular [Todos](http://documentcloud.github.com/backbone/examples/todos/index.html) application 
by [Jérôme Gravel-Niquet](http://jgn.me/) as a starting point for our example, and I'll demonstrate how to leverage Cassette for 
dependency management, combination and minification of assets, font embedding, image embedding, and compilation of JavaScript templates.

Our example application will make use of several open source projects. I recommend you gain a basic understanding of these before you
begin this tutorial:

- [Backbone.js](cumentcloud.github.com/backbone/), [Underscore.js](http://documentcloud.github.com/underscore/) - JavaScript application framework and utilities
- [jQuery](http://jquery.com/) - DOM manipulation and Ajax library
- [Hogan.js](http://twitter.github.com/hogan.js/) - Compilation and rendering of [Mustache](http://mustache.github.com/mustache.5.html) templates

## Setup

```
|-- Fonts
    `-- embed
        `-- <fonts to embed>
|-- Images
    `-- embed
        `-- <images to embed>
|-- Scripts
    |-- lib
        |-- backbone.js
        |-- backbone.localstorage.js
        |-- bundle.txt
        |-- hogan.js
        |-- jquery.js
        |-- json2.js
        `-- underscore.js
    `-- todos
        |-- templates
            |-- item.mustache
            `-- item.mustache
        `-- app.js
|-- Styles
    `-- todos
        `-- app.css
```

## Configuring Cassette

Cassette allows you to separate your assets into separate bundles for JavaScript, CSS and HTML templates. These bundles 
are defined in the `CassetteConfiguration.cs` class.

```cs
public void Configure(BundleCollection bundles, CassetteSettings settings)
{
    // Bundle definitions go here.
}
```

### CSS

Let's add some bundles to the configuration class. First, the CSS:

```cs
bundles.Add<StylesheetBundle>(
    "styles/todos",
    (bundle) => bundle.Processor = new StylesheetPipeline()
        .EmbedImages(path => path.Contains("/embed/"))
        .EmbedFonts(path => path.Contains("/embed/"))
);
```

In this bundle, we've specified that all the stylesheets in `styles/todos` should be included. Additionally, we would like Cassette to
embed both fonts and images as Data URIs. The methods `EmbedImages` and `EmbedFonts` accept an optional anonymous function that allows you to specify
which images should be embedded. Here, we've told Cassette to only embed assets located in a `/embed/` directory.

### JavaScript

Now we'll add the JavaScript bundles:

```cs
bundles.Add<ScriptBundle>(
    "scripts/lib"
);
bundles.Add<ScriptBundle>(
    "scripts/todos"
);
```

Here we've defined two bundles, one for re-usable library code and one for our app-specific code.

### HTML Templates

Finally, we'll create a bundle for our HTML templates:

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
 using Cassette's `HoganPipeline`. This pipeline will find Mustache templates located in the `scripts/todos/template` 
 directory, compile them with Hogan.js and serve them as a single cacheable JavaScript file. Each template will be 
 compiled into a JavaScript function and added to a globally accessible object we've named `JST`. All we need to do to 
 render a template on the client is call `JST['view-name'].render(data)`, where "view-name" corresponds to the filename
 of the template.

## Managing Dependencies

At this point, we've crated four bundle definitions.

- `styles/todos`
- `scripts/lib`
- `scripts/todos`
- `scripts/todos/templates`

We could reference each of these in our view, but instead we'll use Cassette to define the dependencies for our application.

### File-level dependencies

Cassette allows you to include references to other files and bundles in any asset file. Let's add the following to the top of 
`app.js`:

```js
// @reference templates
// @reference ../lib
// @reference ~/styles/todos
```

Here, we've added references to the three other bundles we defined in our configuration class. Cassette will now include these
whenever we reference `scripts/todos`.

## Bundle-level dependencies

If you were to load the `scripts/lib` bundle at this point, you would get a JavaScript error `_ is undefined` because Backbone 0.9.1 depends on 
Underscore. We could add dependency references to each assett, but instead we'll add a `bundle.txt` file to the `scripts/lib` 
directory to define the load order for these assets.

```
json2.js
jquery.js
underscore.js
backbone.js
backbone.localstorage.js
hogan.js
```

## The View

Our application will be rendered from a single Razor view.

```html
<head>
    
    @* Reference the application *@
    @{ Bundles.Reference("scripts/todos"); }
    
    @* Render the assets *@
    @Bundles.RenderStylesheets()
    @Bundles.RenderScripts()
    @Bundles.RenderHtmlTemplates()
    
</head>
```

## The Rendered View

```html
<head>
    <link href="/_cassette/stylesheetbundle/styles/todos_9335a5de5ddf3e78839522a70c90f9c7a6af79ed" type="text/css" rel="stylesheet"/>
    <script src="/_cassette/scriptbundle/scripts/lib_1f020636688f8da6330a28db657b7d468aa311bb" type="text/javascript"></script>
    <script src="/_cassette/scriptbundle/scripts/todos_630d0ac001f5bd947fdf59efd5d714694aa3acf4" type="text/javascript"></script>
    <script src="/_cassette/htmltemplatebundle/scripts/todos/templates_9e1826bef8469fe6de63b9a6991a16164f7af93f" type="text/javascript"></script>
</head>
```
