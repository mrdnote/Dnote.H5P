# Dnote.H5P
H5P implementation for .NET (Standard 2.0)

# Installation
- Install Dnote.H5P NuGet
- Decide how you want to store the metadata of the H5P content (this is info about the libraries required by the content)
  - Use the Linq2Sql implementation provided (you will need to add specific tables to your database) -> Install Dnote.H5P.NetFW.Linq2Sql NuGet (as the name states, this project uses .NET Framework, version 4.7.2 to be specific)
  - Use the EF implementation provided -> (you will need to add specific tables to your database) -> Install Dnote.H5P.EF NuGet
  - Roll your own by subclassing H5PMetaDataAgent -> No need to install a NuGet package
- Decide how you want to store the files of the H5P content and libraries
  - Filesystem inside the website: Use the H5PFileStorageAgent class -> No need to install a NuGet package
  - Azure Storage -> Install Dnote.H5P.AzureStorage NuGet
  - Roll your own by subclassing H5PStorageAgent -> No need to install a NuGet package
- Add the standard H5P includes to your website projects and reference them in your views. You can find these files in the test website Dnote.H5P.NetFW.TestWebSite.
  - /Content/h5p-*.css
  - /Content/h5p.css
  - /fonts/h5p-core-26.*
  - /Scripts/jquery.js
  - /Scripts/request-queue.js
  - /Scripts/h5p-*.js
  - /Scripts/h5p.js
  PS See Dnote.H5P.NetFW.TestWebSite/Views/Home/Exercise.cshtml on how to reference these files
- In the view where you want to render the H5P content:
  - Insert the H5P css files using @Html.H5PCssIncludes(...)
  - Insert the H5P content placeholder using @Html.H5PContent(...)
  - Insert the H5P script using @Html.H5PScript(...)
  - Insert the H5P script files using @Html.H5PJsIncludes(...)
- An example on how to upload a H5P content file can be found in Dnote.H5P.NetFW.TestWebSite.HomeController.Index (the POST action).