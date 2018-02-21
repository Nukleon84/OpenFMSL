# OpenFMSL
The Open Flowsheet Modeling Library is a collection of C# libraries that can be used to simulate stationary chemical processes using mass- and energy balances. The library includes a basic implementation of the IKCAPE Thermodynamics and a handful of simple unit operations. The resulting equation system is solved simulatenously using a Newton-type solver using the L/U decomposition provided in CSPARSE.net.

## Prerequisites
* Visual Studio
* MSBuild
* Nuget Package Manager

## Set up
As the build script is still hard-coded to my development computer, you must build everything manually for the moment. The project is setup using an enterprise component architecture model, so building from scratch is rather simple. Just follow the steps given below.

1. Clone repository on your hard drive
2. Build the solution ./source.contracts/OpenFMSL.contracts/OpenFMSL.contracts.sln. This will create a bin.contracts folder in the project root that all the components in the source folder reference to.
3. Build all the solution in the ./source/ folder in any order you like. The binaries are put into the deploy/exe or deploy/exe_debug folder.
4. Execute shell.exe in the deploy folder to launch the simple IDE.
   
## Getting started
