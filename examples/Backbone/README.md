NOTE: The information in this post is based on a pre-beta release of Cassette 1.1. This is a work in progress. - [@ctcliff](http://twitter.com/ctcliff)

# Managing Backbone.js applications with Cassette.

This post will explain how to manage a single-page Backbone.js application using [Cassette](http://getcassette.net), an asset bundling library for .NET.
We'll use the popular [Todos](http://documentcloud.github.com/backbone/examples/todos/index.html) application 
by [Jérôme Gravel-Niquet](http://jgn.me/) as a starting point for our example. The [source code](https://github.com/christophercliff/cassette/tree/backbone/examples/Backbone) is available on Github, along with this
tutorial.

We'll address the following problems in this tutorial:

### Minimize HTTP requests and file size

This one is probably a no-brainer at this point, but excessive HTTP requests and unnecessary bytes make your app slower.
Cassette will combine and minify our JavaScript and CSS files.

### [FOUC](http://en.wikipedia.org/wiki/Flash_of_unstyled_content) Looks Bad

This effect is especailly offensive with web fonts. We'll use Cassette to encode these assets and include them in our CSS.

### DOM Access and Template Compilation Are Expensive

If we were to include our JavaScript templates inline as HTML, we would need to query the DOM and compile the templates on the client in order
to get them to a usable state. These steps typically look something like this:

```
// Wait for DOM ready.
$(function(){
    // Select the template and compile.
    var myTemplate = _.($('#my-template').html());
    // Do stuff.
});
```

Using Cassette, we are able to perform these steps on the server prior to page load. This approach allows us to organize our templates
as individual files, and has the added benefit of template content being served as JavaScript that can be cached by the browser.

### Dependency Management

Large applications may have many dependent JavaScript modules and CSS components. We'll use cassette to automate dependency management, and
organize our assets into bundles for sensible caching.

## Background Info

Our example application will make use of several open source projects. This tutorial will be more useful if you have a basic understanding
of the following:

- [Backbone.js](cumentcloud.github.com/backbone/), [Underscore.js](http://documentcloud.github.com/underscore/) - JavaScript application framework and utilities
- [jQuery](http://jquery.com/) - DOM manipulation and Ajax library
- [Hogan.js](http://twitter.github.com/hogan.js/) - Compilation and rendering of [Mustache](http://mustache.github.com/mustache.5.html) templates

## Setup

Our filesystem will look like this:

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
embed both fonts and images as Data URIs. The methods `EmbedImages` and `EmbedFonts` accept an optional anonymous function that allows you
to specify which images should be embedded. Some images are not suitable for embedding (e.g. large images)--here, we've told Cassette
to only embed assets located in a `/embed/` directory.

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

We've told Cassette to process template files using the `HoganPipeline`. This pipeline will find Mustache templates located in the
`scripts/todos/template` directory. The templates are compiled to JavaScript on the server using Hogan.js, then combined and served 
as a single file.

The `HoganPipeline` attaches each template to a global object that we can access from our Backbone views. We've specified the name of this
global object to be `JST`. A common pattern for using the `HoganPipeline` with Backbone views might look something like this:

```
className: 'myView',

initialize: function () {
    
    this.template = JST[this.className];
    
}

render: function () {
    
    this.template.render(this.model.toJSON());
    
    return this;
}
```

## Managing Dependencies

At this point, we've created four bundle definitions.

- `styles/todos`
- `scripts/lib`
- `scripts/todos`
- `scripts/todos/templates`

We could reference each of these in our view, but instead we'll use Cassette to handle the dependencies for our application.

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

We'll use a Razor view to render our application. Because we've already specified our dependencies, we only need to include a Bundle
reference for the JavaScript application, `scripts/todos`. Cassette will include the dependent bundles automatically.

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

When our page is loaded in a production environment, each bundle will be loaded rendered as a `link` or `script` tag.

```html
<head>
    <link href="/_cassette/stylesheetbundle/styles/todos_9335a5de5ddf3e78839522a70c90f9c7a6af79ed" type="text/css" rel="stylesheet"/>
    <script src="/_cassette/scriptbundle/scripts/lib_1f020636688f8da6330a28db657b7d468aa311bb" type="text/javascript"></script>
    <script src="/_cassette/scriptbundle/scripts/todos_630d0ac001f5bd947fdf59efd5d714694aa3acf4" type="text/javascript"></script>
    <script src="/_cassette/htmltemplatebundle/scripts/todos/templates_9e1826bef8469fe6de63b9a6991a16164f7af93f" type="text/javascript"></script>
</head>
```
