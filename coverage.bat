@ECHO OFF

rmdir coverage /s /q
del results.xml
del TestResult.xml
.\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:Path64 "-excludebyattribute:*.ExcludeFromCodeCoverage*" "-filter:+[WindowSelector]* -[*Test]* -[WindowSelector]*Annotations.*" "-target:.\packages\NUnit.ConsoleRunner.3.5.0\tools\nunit3-console.exe" "-targetargs: .\tests\WindowSelector.Tests\bin\Debug\WindowSelector.Tests.dll"

.\packages\ReportGenerator.2.5.1\tools\ReportGenerator.exe "-reports:results.xml" "-targetdir:.\coverage"

.\coverage\index.htm
@ECHO ON