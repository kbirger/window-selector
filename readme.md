# Prerequisites
* [The .NET 4.6 Framework](http://go.microsoft.com/fwlink/?LinkId=528259)
* [The Microsoft Build Tools 2015](http://www.microsoft.com/en-us/download/details.aspx?id=48159)
* [The .NET Framework 4.6 targeting Pack](http://go.microsoft.com/fwlink/?LinkId=528261)

# Building

Run the following command to build debug artifacts, run unit tests, and generate coverage
    
    ./build.cmd
    
Run the following command to just do code coverage (can be run from clean solution)

    
    ./build.cmd UnitTestCoverage
    
Alternately, if you have already built, you may use `coverage.bat` instead.