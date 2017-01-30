@ECHO OFF
SETLOCAL

SET serverFolder=\\rufc-devbuild.cneu.cnwk\d$\NetBuild\Server

XCOPY "CCNet.ShortDateLabeller.Plugin\bin\Release\CCNet.ShortDateLabeller.Plugin.dll" "%serverFolder%" /D /Y

XCOPY "CCNet.NetBuildQueue.Plugin\bin\Release\CCNet.NetBuildQueue.Plugin.dll" "%serverFolder%" /D /Y
XCOPY "CCNet.NetBuildQueue.Plugin\bin\Release\Dapper.dll" "%serverFolder%" /D /Y
XCOPY "CCNet.NetBuildQueue.Plugin\bin\Release\NetBuild.Common.dll" "%serverFolder%" /D /Y
XCOPY "CCNet.NetBuildQueue.Plugin\bin\Release\NetBuild.Queue.Core.dll" "%serverFolder%" /D /Y
XCOPY "CCNet.NetBuildQueue.Plugin\bin\Release\Newtonsoft.Json.dll" "%serverFolder%" /D /Y

PAUSE
