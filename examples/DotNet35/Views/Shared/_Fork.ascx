﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<%
    // This is an example of an external script reference. Cassette will generate the <script>
    // tag for us. In this example, we've set the location to be "body" so it'll appear at the
    // end of the page, just before </body>. (See _Layout.cshtml for the rendering calls.)
    Bundles.Reference("http://gitforked.com/api/1.1/button.js", "body");
%>
<p>Cassette is free, open source, software: 
    <a href="https://github.com/andrewdavey/cassette" class="gitforked-button gitforked-forks gitforked-watchers">Fork</a>
</p>