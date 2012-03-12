﻿<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Example Web App
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    // Pass the contents of the Model to the client code via an inline script block.
    Bundles.AddPageData("pageViewData", Model);
    
    // Reference the modules this page needs.
    Bundles.Reference("scripts/color-picker");
%>
<div class="color-picker">
    <h2>Color Picker</h2>
    <table>
        <tr>
            <th>Red</th>
            <%-- We've added a 'slider' binding to Knockout's built-in object. See slider.js --%>
            <td><div class="color-slider" data-bind="slider: red"></div></td>
            <td style="width: 50px">
                <span data-bind="text: red"></span>
            </td>
        </tr>
        <tr>
            <th>Green</th>
            <td><div class="color-slider" data-bind="slider: green"></div></td>
            <td data-bind="text: green"></td>
        </tr>
        <tr>
            <th>Blue</th>
            <td><div class="color-slider" data-bind="slider: blue"></div></td>
            <td data-bind="text: blue"></td>
        </tr>
        <tr>
            <th>Color</th>
            <td colspan="2">
                <div class="chosen-color" data-bind="style: { backgroundColor: color }"></div>
                <div class="chosen-color-hex" data-bind="text: color"></div>
            </td>
        </tr>
        <tr>
            <th> </th>
            <td>
                <button data-bind="click: random">Random</button>
                <button data-bind="click: save">Save Color</button>
            </td>
        </tr>
    </table>
</div>
<div class="saved-colors">
    <h2>Saved Colors</h2>
    <p data-bind="visible: savedColors().length === 0">
        Choose some colors...
    </p>
    <ul data-bind="template: { foreach: savedColors, name: 'ColorListItem' }"></ul>
</div>
</asp:Content>
