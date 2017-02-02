1. Locate TFS plugin directory:
-------------------------------
C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins

2. Copy the following files:
----------------------------
- Atom.Module.Configuration.dll
- NetBuild.Common.dll
- NetBuild.Queue.Client.dll
- NetBuild.Queue.Core.dll
- Newtonsoft.Json.dll
- TfsPlugin.NetBuildQueue.dll
- TfsPlugin.NetBuildQueue.dll.config

3. Make sure TfsPlugin.NetBuildQueue.dll.config has all values specified properly:
----------------------------------------------------------------------------------
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<appSettings>
		<add key="Debug.Enabled" value="false" />
		<add key="NetBuild.ServerUrl" value="http://my-queue-server:8001/" />
		<add key="NetBuild.Timeout" value="00:00:15" />
	</appSettings>
</configuration>
