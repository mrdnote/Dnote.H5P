# Dnote.H5P
H5P implementation for .NET. It's written in .NET Standard 2.0, so usable in .NET Framework, .NET Core and .NET 5.x.

This library takes care of processing and storing H5P files into the filesystem or Azure (you can add your own storage implementation) and also takes care of rendering basic javascript in your
views. The library does *not* take care of processing answers and results from H5P content items, nor does it support H5P editor integration.

# Installation
Check out the Dnote.H5P.NetFW.TestWebSite on how to use H5P in your website.

# Projects
## Dnote.H5P
The main library project, useable in .NET Framework, .NET Core and .NET 5.x.
## Dnote.H5P.Test
Unit test for the Dnote.H5P library.
## Dnote.H5P.NetFW.UI
UI layer for displaying H5P content in a .NET Framework website.
## Dnote.H5P.Core.UI
TODO. UI layer for displaying H5P content in a .NET Core and .NET 5.x website.
## Dnote.H5P.NetFW.TestWebSite
.NET Framework Website to test the H5P library and to see how it all hooks up.

# Note
When using Azure Storage, make sure you have enabled CORS on the blob container served else you will get errors when for example fonts are downloaded by the H5P content.
