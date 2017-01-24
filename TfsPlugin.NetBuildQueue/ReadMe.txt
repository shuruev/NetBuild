1. Locate TFS plugin directory:
-------------------------------
C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins

2. Copy the following files:
----------------------------
- Atom.Module.Configuration.dll
- NetBuild.Common.dll
- Newtonsoft.Json.dll
- TfsPlugin.NetBuildQueue.dll
- TfsPlugin.NetBuildQueue.dll.config

3. Make sure TfsPlugin.NetBuildQueue.dll.config has all values specified properly:
----------------------------------------------------------------------------------
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<appSettings>
		<add key="Security.Thumbprint" value="426784afb9972e3a1ac3d3462d96319e2fb077fd"/>
		<add key="NetBuild.DbConnection" value="Server=sqlserver.azure.net; Database=NetBuild; User Id=sqluser; Password={password};"/>
		<add key="NetBuild.DbPassword" value="U6uR9mxBsO44L2uBJc6yA3WYwrA+8nImi96VVdctc2Uypybl71YlxHGvVeE66a2xIWpOFcKOIEaNS+BWZ39LcBlJ9yRWtuhjPo7Qmvnb1KTLNjMmhaAHEN4T6aukbtjh2xmyOB304CyxVWRWuo+FffO2LLHu2dwj6EjLx/BtgfI="/>
		<add key="NetBuild.DbTimeout" value="00:00:15" />
	</appSettings>
</configuration>
