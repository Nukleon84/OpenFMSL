# OpenFMSL
The Open Flowsheet Modeling Library is a collection of C# libraries that can be used to simulate stationary chemical processes using mass- and energy balances. The library includes a basic implementation of the IKCAPE Thermodynamics and a handful of simple unit operations. The resulting equation system is solved simulatenously using a Newton-type solver using the L/U decomposition provided in CSPARSE.net.

This program was build for fun and the enjoyment of exploring process simulation tools. It was heavily inspired by the PhD thesis of K.Lau and G. Varma.

Development of a process simulator using object oriented programming: Information modeling and program structure
Gadiraju V. Varma
https://lib.dr.iastate.edu/cgi/viewcontent.cgi?article=11354&context=rtd

Development of a process simulator using object oriented programming:  Numerical procedures and convergence studies
Kheng Hock Lau
https://lib.dr.iastate.edu/cgi/viewcontent.cgi?article=11324&context=rtd

The thermodynamics methods implemented in this libary are part of the IKCAPE thermodynamics. I reimplemented the equations in my own modeling framework. I also use the neutral input format described in their user guide as an input format.
http://dechema.de/en/IK_CAPE+THERMO-p-40.html

If you are interested in Process Simulation software, make sure to check out the stellar DWSIM project by Daniel Medeiros: https://github.com/DanWBR/dwsim5

Disclaimer: This is my first real GitHub project. I apologize for any things that I forgot to add. If you find some things are missing (like Licenses for libraries), please notify me. I will add them as soon as possible.

## Prerequisites
* Visual Studio
* MSBuild
* Nuget Package Manager

## Set up
As the build script is still hard-coded to my development computer, you must build everything manually for the moment. The project is setup using an enterprise component architecture model, so building from scratch is rather simple. Just follow the steps given below.

1. Clone the repository on your hard drive
2. Build the solution ./source.contracts/OpenFMSL.contracts/OpenFMSL.contracts.sln. This will create a bin.contracts folder in the project root that all the components in the source folder reference to.
3. Build all the solutions in the ./source/ folder in any order you like. The binaries are put into the deploy/exe or deploy/exe_debug folder. There are no dependencies between the component projects.
4. Execute shell.exe in the deploy folder to launch the simple IDE.
   
## Getting started
1. Once the program has started, click the Load button (second from the left) to select the BasicExample.project file.
2. Double-click the model "BTX column" from the list
![alt text](https://github.com/Nukleon84/OpenFMSL/blob/master/doc/IDE_Model.PNG "The IDE with a model loaded")
3. Press the RUN button on the toolbar to execute the script.
![alt text](https://github.com/Nukleon84/OpenFMSL/blob/master/doc/IDE_console.PNG "The IDE has a simple python shell integrated that reports results and takes interactive commands")
![alt text](https://github.com/Nukleon84/OpenFMSL/blob/master/doc/IDE_plots.PNG "The IDE can also display simple charts and diagrams")

## Going further
You can also build your own programs that use the different components of OpenFMSL. You should strongly consider also using Castle.Windsor as the dependency injection container, as the library was build around this very nice and powerful libary. As a starting point, you could create a command line program that uses the ThermodynamicsImporter and the basic classes in the library to run a fixed flowsheet. You can copy a lot of the Python code from the examples directly to C# (just add semicolons). 

## Next Steps
1. Write better documentation/WIKI
2. Add some more examples
3. Properly tidy up the build script to make it run from any PC
