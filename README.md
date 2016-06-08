#Design Automation API: Input/Output worflow .NET sample

[![.net](https://img.shields.io/badge/.net-4.5-green.svg)](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
[![odata](https://img.shields.io/badge/odata-4.0-yellow.svg)](http://www.odata.org/documentation/)
[![ver](https://img.shields.io/badge/Design%20Automation%20API-2.0-blue.svg)](https://developer.autodesk.com/api/autocadio/v2/)
[![visual studio](https://img.shields.io/badge/Visual%20Studio-2012%7C2013-brightgreen.svg)](https://www.visualstudio.com/)
[![License](http://img.shields.io/:license-mit-red.svg)](http://opensource.org/licenses/MIT)

##Description
This C# sample shows various ways to specify input for a workitem

##Dependencies

Visual Studio 2012, 2013. 2015 should be also fine, but has not yet been tested.

##Setup/Usage Instructions

* Restore the packages of the project by [NuGet](https://www.nuget.org/). The simplest way is to Projects tab >> Enable NuGet Package Restore. Then right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
* Apply credencials of Design Automation API from https://developer.autodesk.com/. Put your consumer key and secret key at  line 19 and 20 of [program.cs](./Program.cs) 
*  Run project, you will see a status in the console:
* if everything works well, some result files (pdf, zip) and the report files will be downloaded at **MyDocuments**.
* if there is any error with the process, check the report file what error is indicated.

[![](RunDemo.png)] 
Please refer to [Design Automation API v2 API documentation](https://developer.autodesk.com/api/autocadio/v2/).

## Questions

Please post your question at our [forum](http://forums.autodesk.com/t5/autocad-i-o/bd-p/105).

## License

These samples are licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

##Written by 

Jonathan Miao & Albert Szilvasy
