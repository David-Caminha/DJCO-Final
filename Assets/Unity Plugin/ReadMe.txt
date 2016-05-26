==================================
Unity Plugin Installation
Author:Sceelix
==================================

Installation:
1. Open or create a new project in Unity.
2. Make sure that under Edit-> Project Settings ->Player->Other Settings -> Optimization->Api Compatibility Level is set to ".NET 2.0" (and not just subset)
3. Add the contents of this folder to the 'Assets' folder of your project (this Readme is not needed)
4. Assuming no build errors occur, a new entry in the top menu, called "Sceelix", should appear. The plugin is now installed.

Connecting with Sceelix Designer:
1. The Sceelix designer must be running first 
2. In Unity, select Sceelix->Connect to Designer from the top menu. A socket connection will be established and data can be sent from Sceelix to Unity.

Note: Unity is not very stable when using socket connections from within the editor, meaning it can sometimes block and crash. 
To avoid some of these cases, our plugin may need to close the connection. If data is not received from Sceelix, you might need to connect again.
