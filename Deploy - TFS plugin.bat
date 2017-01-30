@ECHO OFF
SETLOCAL

SET pluginFolder=\\rufc-devbuild.cneu.cnwk\c$\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins

XCOPY "TfsPlugin.NetBuildQueue\bin\Release\Atom.Module.Configuration.dll" "%pluginFolder%" /D /Y
XCOPY "TfsPlugin.NetBuildQueue\bin\Release\Dapper.dll" "%pluginFolder%" /D /Y
XCOPY "TfsPlugin.NetBuildQueue\bin\Release\NetBuild.Common.dll" "%pluginFolder%" /D /Y
XCOPY "TfsPlugin.NetBuildQueue\bin\Release\NetBuild.Queue.Core.dll" "%pluginFolder%" /D /Y
XCOPY "TfsPlugin.NetBuildQueue\bin\Release\Newtonsoft.Json.dll" "%pluginFolder%" /D /Y
XCOPY "TfsPlugin.NetBuildQueue\bin\Release\TfsPlugin.NetBuildQueue.dll" "%pluginFolder%" /D /Y

PAUSE
