# **NeoMonitor-Server**

[![.NET Core](https://img.shields.io/badge/.NET%20Core-%203.1-brightgreen)][DotNetCoreUrl]

[DotNetCoreUrl]: https://dotnet.microsoft.com/download

## **Introduction**

**This is a repo storing source codes of NeoMonitor server.**

## **Quick Start**

### Preparation

| Name | Version |
| :-: | :-: |
| DotNet Core SDK | __3.1+__ |
| EntityFramework Core | __3.1+__ |
| Visual Studio 2019 | __16.5+__ |
| MySQL | __8.0+__ |

### Steps

1. Check the *ConnectionStrings* among [appsettings.json](https://github.com/alienworks/NeoMonitor-Server/blob/master/NodeMonitor/appsettings.json). Replace it with your own setting if necessary.

2. Open the ***Package Manager Console*** and set the default project as `Data\NeoMonitor.Data`.

3. Execute the command line: `Add-Migration neonodes_local_v1.4.0 -c NeoMonitorContext`, and wait for the end of migration files creating.

4. Execute the command line: `Update-Database`, and then check the database whether the data tables are created.

5. Run the project.
