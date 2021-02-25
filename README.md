# Serverino - It's a simple way to host your applications
![Build Serverino](https://github.com/AugustoDeveloper/serverino/workflows/Build%20Serverino/badge.svg)

## Features
- Host multiples application on server
- Compatible with Asp.Net Core MVC
- Runtime update application hosted

## How it works
When you run the `Serverino.Watch`, it will create a folder on 
executable path, called `apps`. All the application that need 
host, you need create a inner folder with name of library, for example:
```
Serverino.Watch.exe
/apps
└───/SampleApp
    └───SampleApp.dll
    └───appsettings.json
```
This will host a SampleApp application on some port, the port is in configuration file.    
Every change on this inner folder, will trigger an update process and the hosted application
 will restart.
  
## How to create an web api
First of all, create a .Net Core Library and set this library as `net5.0` and add the reference
`Microsoft.AspNetCore.App`, this way the 
library goes to resolve dependencies on compile for your web application.
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
</Project>
```
Now, crate a folder with name `Controllers` and add all your controllers class into this folder:
```c#
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SampleApp.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class BooksController : ControllerBase
    {
        [HttpGet("all")]
        async public Task<IActionResult> GetAllBooks([FromServices]IConfiguration configuration)
            => await Task.FromResult(Ok(configuration.GetValue<string>("booksResult", "Didn't came from configuration file")));
    }
}
```
Now, we need create a configuration file, this file must have some configuration for the host application,
as we said on [How it Works](#how-it-works) section. So, create a file on project and use the name
`appsettings.json`, after this add on project(edit *.csproj) to always copy:
```xml
  <ItemGroup>
    <None Include="appsettings.json" CopyToOutputDirectory="Always" />
  </ItemGroup>
```
And the content of your configuration file must have somenthing like this:
```json
{
    "port": "$PORT",
    "booksResult": "This is a value from appsettings"
}
```
Change the value `$PORT` for port of your application needs to listen. That's it! Now compile the project,
run the `Serverino.Watch.exe`, drop the output of your app-library into folder at `apps` folder and watch
the show!
