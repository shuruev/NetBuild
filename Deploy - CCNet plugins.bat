@ECHO OFF
SETLOCAL

SET serverFolder=\\rufc-devbuild.cneu.cnwk\d$\NetBuild\Server

XCOPY "CCNet.ShortDateLabeller.Plugin\bin\Debug\CCNet.ShortDateLabeller.Plugin.dll" "%serverFolder%" /D /Y

XCOPY "CCNet.NetBuildQueue.Plugin\bin\Debug\CCNet.NetBuildQueue.Plugin.dll" "%serverFolder%" /D /Y
XCOPY "CCNet.NetBuildQueue.Plugin\bin\Debug\Dapper.dll" "%serverFolder%" /D /Y
XCOPY "CCNet.NetBuildQueue.Plugin\bin\Debug\NetBuild.Common.dll" "%serverFolder%" /D /Y
XCOPY "CCNet.NetBuildQueue.Plugin\bin\Debug\NetBuild.Queue.Core.dll" "%serverFolder%" /D /Y

PAUSE
