@ECHO OFF
SETLOCAL

SC \\rufc-devbuild.cneu.cnwk STOP "[NETBUILD] Queue Server"

SET targetFolder=\\rufc-devbuild.cneu.cnwk\d$\NetBuild\NetBuild.Queue.Server\

XCOPY "NetBuild.Queue.Server\bin\Release\Atom.Module.Configuration.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Dapper.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Microsoft.Owin.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Microsoft.Owin.Host.HttpListener.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Microsoft.Owin.Hosting.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\NetBuild.Common.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\NetBuild.Queue.Core.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\NetBuild.Queue.Engine.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\NetBuild.Queue.Server.exe" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Newtonsoft.Json.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Owin.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Serilog.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\Serilog.Sinks.Literate.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\SimpleInjector.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\SimpleInjector.Extensions.ExecutionContextScoping.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\SimpleInjector.Integration.WebApi.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\System.Net.Http.Formatting.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\System.Web.Http.dll" "%targetFolder%" /D /Y
XCOPY "NetBuild.Queue.Server\bin\Release\System.Web.Http.Owin.dll" "%targetFolder%" /D /Y

SC \\rufc-devbuild.cneu.cnwk START "[NETBUILD] Queue Server"

PAUSE
